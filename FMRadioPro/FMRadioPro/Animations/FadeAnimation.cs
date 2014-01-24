using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;


namespace Utility.Animations
{
    public class FadeAnimation : AnimationBase
    {
        #region Constructor

        public FadeAnimation()
        {
            Init();
        }

        #endregion

        #region Properties

        private DoubleAnimationUsingKeyFrames _Animation = null;
        private EasingDoubleKeyFrame _KeyFrame_from = null;
        private EasingDoubleKeyFrame _KeyFrame_to = null;

        private static Stack<FadeAnimation> AnimationPool = new Stack<FadeAnimation>();

        private double TargetOpacity = 0;

        #endregion

        #region Animation

        private void Init()
        {
            _Storyboard = new Storyboard();
            _Storyboard.Completed += _Storyboard_Completed;

            /***animation x***/
            _Animation = new DoubleAnimationUsingKeyFrames();

            /*key frame 1*/
            _KeyFrame_from = new EasingDoubleKeyFrame();
            _KeyFrame_from.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
            _KeyFrame_from.Value = 0;
            _Animation.KeyFrames.Add(_KeyFrame_from);

            /*key frame 2*/
            _KeyFrame_to = new EasingDoubleKeyFrame();
            _KeyFrame_to.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1));
            _KeyFrame_to.Value = 0;
            _Animation.KeyFrames.Add(_KeyFrame_to);

            Storyboard.SetTargetProperty(_Animation,new PropertyPath( "(UIElement.Opacity)"));
            _Storyboard.Children.Add(_Animation);
        }

        public static void Fade(FrameworkElement element, double from, double to, TimeSpan duration, Action<FrameworkElement> completed)
        {
            FadeAnimation animation = null;
            if (AnimationPool.Count == 0)
            {
                animation = new FadeAnimation();
            }
            else
            {
                animation = AnimationPool.Pop();
            }

            animation.Animate(element, from, to, duration, completed);
        }

        private void Animate(FrameworkElement element, double from, double to, TimeSpan duration, Action<FrameworkElement> completed)
        {
            AnimationTarget = element;
            TargetOpacity = to;
            AnimationCompleted = completed;

            if (_Storyboard == null)
            {
                Init();
            }
            else
            {
                _Storyboard.Stop();
            }

            /*time*/
            _KeyFrame_to.KeyTime = KeyTime.FromTimeSpan(duration);

            /*value*/
            _KeyFrame_from.Value = from;
            _KeyFrame_to.Value = to;

            Storyboard.SetTarget(_Animation, element);
            _Storyboard.Begin();
        }

        private void _Storyboard_Completed(object sender, object e)
        {
            AnimationTarget.Opacity = TargetOpacity;
            if (!AnimationPool.Contains(this))
            {
                AnimationPool.Push(this);
            }

            if (AnimationCompleted != null)
            {
                AnimationCompleted(AnimationTarget);
            }
        }

        #endregion

    }
}
