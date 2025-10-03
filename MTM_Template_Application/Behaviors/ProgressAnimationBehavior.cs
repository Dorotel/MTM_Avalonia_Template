using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Xaml.Interactivity;

namespace MTM_Template_Application.Behaviors;

/// <summary>
/// Smooth animation behavior for progress bar to avoid jitter.
/// Animates progress changes using easing functions.
/// </summary>
public class ProgressAnimationBehavior : Behavior<ProgressBar>
{
    /// <summary>
    /// The duration of the animation in milliseconds.
    /// </summary>
    public static readonly StyledProperty<int> AnimationDurationProperty =
        AvaloniaProperty.Register<ProgressAnimationBehavior, int>(
            nameof(AnimationDuration),
            defaultValue: 300
        );

    public int AnimationDuration
    {
        get => GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    /// <summary>
    /// The easing function to use for the animation.
    /// </summary>
    public static readonly StyledProperty<Easing> EasingProperty =
        AvaloniaProperty.Register<ProgressAnimationBehavior, Easing>(
            nameof(Easing),
            defaultValue: new CubicEaseOut()
        );

    public Easing Easing
    {
        get => GetValue(EasingProperty);
        set => SetValue(EasingProperty, value);
    }

    private double _previousValue;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
        {
            AssociatedObject.PropertyChanged += OnProgressBarPropertyChanged;
            _previousValue = AssociatedObject.Value;
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject != null)
        {
            AssociatedObject.PropertyChanged -= OnProgressBarPropertyChanged;
        }
    }

    private void OnProgressBarPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ProgressBar.ValueProperty && AssociatedObject != null)
        {
            var newValue = (double)e.NewValue!;
            var oldValue = _previousValue;

            // Only animate if value increased (don't animate backwards)
            if (newValue > oldValue)
            {
                AnimateProgress(oldValue, newValue);
            }
            else
            {
                // Instant update for backwards or reset
                _previousValue = newValue;
            }
        }
    }

    private void AnimateProgress(double from, double to)
    {
        if (AssociatedObject == null)
            return;

        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(AnimationDuration),
            Easing = Easing,
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(ProgressBar.ValueProperty, from)
                    },
                    Cue = new Cue(0.0)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(ProgressBar.ValueProperty, to)
                    },
                    Cue = new Cue(1.0)
                }
            }
        };

        animation.RunAsync(AssociatedObject);
        _previousValue = to;
    }
}
