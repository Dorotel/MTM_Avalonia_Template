# PDF to Text Converter (CLI)

A simple Windows-friendly Python CLI to convert PDF files into `.txt` files.

Features

- Single file or folder input
- Optional recursive processing
- Output to files or stdout
- Handles many encrypted PDFs (empty-password)
- Uses `pypdf` primarily, falls back to `pdfminer.six`

## Setup (Windows PowerShell)

1) Create and activate a virtual environment (recommended)

```powershell
python -m venv .venv
. .venv\Scripts\Activate.ps1
```

2) Install dependencies

```powershell
pip install -r requirements.txt
```

## Usage

- Convert a single file and write next to the PDF

```powershell
python tools/pdf_to_text.py "Visual Files/Guides/Reference - Core.pdf"
```

- Convert a folder recursively, outputting to a separate folder

```powershell
python tools/pdf_to_text.py "Visual Files/Guides" --recurse --out out-text
```

- Print to stdout instead of writing a file

```powershell
python tools/pdf_to_text.py "Visual Files/Guides/Reference - Core.pdf" --stdout
```

Tip: Change default encoding if needed (e.g., windows-1252)

```powershell
python tools/pdf_to_text.py "Visual Files/Guides/Reference - Core.pdf" --encoding windows-1252
```

### OCR (for scanned/image PDFs)

If extraction fails because the PDF is image-based, you can try OCR:

```powershell
python tools/pdf_to_text.py "Visual Files/Guides/Reference - Core.pdf" --ocr --out out-text
```

Requirements for OCR:

- Install Tesseract OCR for Windows (add tesseract.exe to PATH)
- Python packages are already included (pytesseract, pillow)

## Notes

- If `pypdf` fails to extract text on some pages, the tool transparently retries with `pdfminer.six`.
- Image-based/scanned PDFs require OCR, which is out of scope for this script. If you need OCR, consider adding `pytesseract` and Tesseract OCR.
