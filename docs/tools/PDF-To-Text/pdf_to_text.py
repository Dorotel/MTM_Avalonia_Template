#!/usr/bin/env python3
"""
PDF to Text converter CLI

Features:
- Extracts text from a single PDF file or all PDFs in a folder
- Optional recursive folder processing
- Writes .txt files next to PDFs or to a specified output folder
- Prints to stdout with --stdout
- Handles encrypted PDFs when no password is required
- Primary extractor: pypdf; fallback: pdfminer.six for tricky files

Usage examples:
  python tools/pdf_to_text.py "Visual Files/Guides/Guide - User Manual.pdf"
  python tools/pdf_to_text.py "Visual Files/Guides" --out "out-text" --recurse
  python tools/pdf_to_text.py "Visual Files/Guides/Reference - Core.pdf" --stdout
"""
from __future__ import annotations
import argparse
import sys
import os
import shutil
from pathlib import Path
import tempfile
import subprocess

# Prefer lightweight dependency first
try:
    from pypdf import PdfReader
except Exception:  # pragma: no cover - handled at runtime
    PdfReader = None  # type: ignore

# Fallback that is heavier but more robust on some PDFs
try:
    from pdfminer.high_level import extract_text as pdfminer_extract_text
except Exception:  # pragma: no cover
    pdfminer_extract_text = None  # type: ignore

# Third fallback: PyMuPDF is robust on many PDFs
try:
    import fitz  # PyMuPDF
except Exception:  # pragma: no cover
    fitz = None  # type: ignore

# Optional OCR dependencies via pytesseract (used if available)
try:
    import pytesseract  # type: ignore
    from PIL import Image
except Exception:  # pragma: no cover
    pytesseract = None  # type: ignore
    Image = None  # type: ignore

def read_text_with_pypdf(pdf_path: Path) -> str | None:
    if PdfReader is None:
        return None
    try:
        reader = PdfReader(str(pdf_path))
        if getattr(reader, "is_encrypted", False):
            # Try to decrypt with empty password; many PDFs allow this
            try:
                reader.decrypt("")  # type: ignore[attr-defined]
            except Exception:
                pass
        texts = []
        for page in reader.pages:
            try:
                texts.append(page.extract_text() or "")
            except Exception:
                # If any page fails, give up so we can try fallback
                return None
        return "\n".join(texts)
    except Exception:
        return None


def read_text_with_pdfminer(pdf_path: Path) -> str | None:
    if pdfminer_extract_text is None:
        return None
    try:
        return pdfminer_extract_text(str(pdf_path))
    except Exception:
        return None

def read_text_with_pymupdf(pdf_path: Path) -> str | None:
    if fitz is None:
        return None
    try:
        doc = fitz.open(str(pdf_path))
        # Try empty password if encrypted
        if getattr(doc, "needs_pass", False):
            try:
                doc.authenticate("")
            except Exception:
                pass
        texts: list[str] = []
        for page in doc:
            try:
                texts.append(page.get_text("text") or "")
            except Exception:
                return None
        return "\n".join(texts)
    except Exception:
        return None


def extract_text_any(pdf_path: Path) -> str:
    # Try pypdf first
    text = read_text_with_pypdf(pdf_path)
    if text is None or not text.strip():
        # Fallback to pdfminer
        text = read_text_with_pdfminer(pdf_path)
    if text is None or not text.strip():
        # Final fallback to PyMuPDF
        text = read_text_with_pymupdf(pdf_path)
    if text is None:
        raise RuntimeError(f"Failed to extract text from: {pdf_path}")
    return text


def ocr_with_pymupdf_pytesseract(pdf_path: Path) -> str | None:
    """OCR using PyMuPDF rendering + pytesseract (if installed)."""
    if fitz is None or pytesseract is None or Image is None:
        return None
    try:
        doc = fitz.open(str(pdf_path))
        texts: list[str] = []
        for page in doc:
            pix = page.get_pixmap(dpi=200)
            img = Image.frombytes("RGB", [pix.width, pix.height], pix.samples)
            txt = pytesseract.image_to_string(img)
            texts.append(txt or "")
        return "\n".join(texts)
    except Exception:
        return None


def ocr_with_pymupdf_tesseract_cli(pdf_path: Path, tesseract_cmd: str | None) -> str | None:
    """OCR using PyMuPDF rendering + Tesseract CLI (no pytesseract dependency)."""
    if fitz is None or tesseract_cmd is None:
        return None
    try:
        doc = fitz.open(str(pdf_path))
        texts: list[str] = []
        with tempfile.TemporaryDirectory() as tmpdir:
            tmpdir_path = Path(tmpdir)
            for i, page in enumerate(doc):
                pix = page.get_pixmap(dpi=200)
                img_path = tmpdir_path / f"page_{i}.png"
                pix.save(str(img_path))
                # Run tesseract CLI: tesseract input.png stdout -l eng
                cmd = [tesseract_cmd, str(img_path), "stdout", "-l", "eng"]
                try:
                    res = subprocess.run(cmd, capture_output=True, text=True, check=False)
                    if res.returncode == 0:
                        texts.append(res.stdout or "")
                    else:
                        # include stderr only for debugging; continue to next page
                        texts.append("")
                except Exception:
                    texts.append("")
        combined = "\n".join(texts)
        return combined if combined.strip() else None
    except Exception:
        return None


def convert_one(pdf_path: Path, out_dir: Path | None, to_stdout: bool, encoding: str = "utf-8", use_ocr: bool = False, tesseract_cmd: str | None = None) -> None:
    text: str | None = None
    try:
        text = extract_text_any(pdf_path)
    except Exception:
        # Ignore here; we may try OCR below
        text = None
    if (text is None or not text.strip()) and use_ocr:
        # Try pytesseract path first if available, else CLI fallback
        ocr_text = ocr_with_pymupdf_pytesseract(pdf_path)
        if (ocr_text is None or not ocr_text.strip()):
            ocr_text = ocr_with_pymupdf_tesseract_cli(pdf_path, tesseract_cmd)
        if ocr_text and ocr_text.strip():
            text = ocr_text
    if text is None:
        raise RuntimeError(f"Failed to extract text from: {pdf_path}")
    if to_stdout:
        # Ensure encoding on Windows terminals
        sys.stdout.reconfigure(encoding=encoding)  # type: ignore[attr-defined]
        print(text)
        return

    if out_dir is None:
        out_dir = pdf_path.parent
    out_dir.mkdir(parents=True, exist_ok=True)

    out_path = out_dir / (pdf_path.stem + ".txt")
    with out_path.open("w", encoding=encoding, newline="") as f:
        f.write(text)


def find_pdfs(input_path: Path, recurse: bool) -> list[Path]:
    if input_path.is_file():
        return [input_path]
    pattern = "**/*.pdf" if recurse else "*.pdf"
    return sorted(p for p in input_path.glob(pattern) if p.is_file())


def parse_args(argv: list[str]) -> argparse.Namespace:
    p = argparse.ArgumentParser(description="Convert PDF files to text")
    p.add_argument("path", help="PDF file or folder containing PDFs")
    p.add_argument("--out", "-o", dest="out", help="Output folder for .txt files; defaults to alongside PDFs")
    p.add_argument("--recurse", "-r", action="store_true", help="Recurse into subfolders when path is a directory")
    p.add_argument("--stdout", action="store_true", help="Print extracted text to stdout instead of writing files")
    p.add_argument("--encoding", default="utf-8", help="Text encoding for output files and stdout (default: utf-8)")
    p.add_argument("--ocr", action="store_true", help="Use OCR as a last resort (requires Tesseract)")
    p.add_argument("--tesseract", dest="tesseract", help="Path to tesseract.exe (overrides PATH auto-detection)")
    return p.parse_args(argv)


def main(argv: list[str]) -> int:
    args = parse_args(argv)
    input_path = Path(args.path)
    if not input_path.exists():
        print(f"Error: path not found: {input_path}", file=sys.stderr)
        return 2

    out_dir = Path(args.out).resolve() if args.out else None

    # If OCR requested, try to ensure tesseract is configured
    tesseract_cmd: str | None = None
    if args.ocr:
        # Prefer explicit path, else PATH, else common install dirs
        if args.tesseract:
            cand = Path(args.tesseract)
            if cand.exists():
                tesseract_cmd = str(cand)
        if tesseract_cmd is None:
            which = shutil.which("tesseract")
            if which:
                tesseract_cmd = which
        if tesseract_cmd is None:
            for cand in [
                Path(r"C:\\Program Files\\Tesseract-OCR\\tesseract.exe"),
                Path(r"C:\\Program Files (x86)\\Tesseract-OCR\\tesseract.exe"),
            ]:
                if cand.exists():
                    tesseract_cmd = str(cand)
                    break
        if tesseract_cmd is None:
            print("Warning: --ocr requested but Tesseract not found. Install it or pass --tesseract path.", file=sys.stderr)

    pdfs = find_pdfs(input_path, recurse=args.recurse)
    if not pdfs:
        print("No PDF files found.", file=sys.stderr)
        return 1

    for pdf in pdfs:
        try:
            convert_one(pdf, out_dir, args.stdout, encoding=args.encoding, use_ocr=args.ocr, tesseract_cmd=tesseract_cmd)
            if not args.stdout:
                rel = os.path.relpath(pdf, start=os.getcwd())
                print(f"Converted: {rel}")
        except Exception as ex:
            rel = os.path.relpath(pdf, start=os.getcwd())
            print(f"Failed: {rel} -> {ex}", file=sys.stderr)
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
