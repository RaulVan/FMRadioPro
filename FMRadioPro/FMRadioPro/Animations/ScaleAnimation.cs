using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Windows.Foundation;


namespace Utility.Animations
{
    public class ScaleAnimation : AnimationBase
    {
        #region Constructor

        public ScaleAnimation()
        {
            Init();
        }

        #endregion

        #region Properties

        private DoubleAnimationUsingKeyFrames _Animation_X = null;
        private DoubleAnimationUsingKeyFrames _Animation_Y = null;

        private EasingDoubleKeyFrame _KeyFrame_x_from = null;
        private EasingDoubleKeyFrame _KeyFrame_x_to = null;
        private EasingDoubleKeyFrame _KeyFrame_y_from = null;
        private EasingDoubleKeyFrame _KeyFrame_y_to = null;

        private double TargetX = 0;
        private double TargetY = 0;

        private static Stack<ScaleAnimation> AnimationPool = new Stack<ScaleAnimation>();

        #endregion

        #region Animation

        private void Init()
        {
            _Storyboard = new Storyboard();
            _Storyboard.Completed += _Storyboard_Completed;

            /***animation x***/
            _Animation_X = new DoubleAnimationUsingKeyFrames();

            /*key frame 1*/
            _KeyFrame_x_from = new EasingDoubleKeyFrame();
            _KeyFrame_x_from.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
            _KeyFrame_x_from.Value = 0;
            _Animation_X.KeyFrames.Add(_KeyFrame_x_from);

            /*key frame 2*/
            _KeyFrame_x_to = new EasingDoubleKeyFrame();
            _KeyFrame_x_to.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1));
            _KeyFrame_x_to.Value = 1;
            _Animation_X.KeyFrames.Add(_KeyFrame_x_to);

            Storyboard.SetTargetProperty(_Animation_X, new  PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleX)"));
            _Storyboard.Children.Add(_Animation_X);

            /***animation y***/
            _Animation_Y = new DoubleAnimationUsingKeyFrames();

            /*key frame 1*/
            _KeyFrame_y_from = new EasingDoubleKeyFrame();
            _KeyFrame_y_from.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
            _KeyFrame_y_from.Value = 0;
            _Animation_Y.KeyFrames.Add(_KeyFrame_y_from);

            /*key frame 2*/
            _KeyFrame_y_to = new EasingDoubleKeyFrame();
            _KeyFrame_y_to.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1));
            _KeyFrame_y_to.Value = 1;
            _Animation_Y.KeyFrames.Add(_KeyFrame_y_to);

            Storyboard.SetTargetProperty(_Animation_Y, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleY)"));
            _Storyboard.Children.Add(_Animation_Y);
        }

        public static ScaleAnimation ScaleFromTo(FrameworkElement cell,
                double from_x, double from_y,
                double to_x, double to_y,
                TimeSpan duration, Action<FrameworkElement> completed)
        {
            ScaleAnimation animation = null;
            if (AnimationPool.Count == 0)
            {
                animation = new ScaleAnimation();
            }
            else
            {
                animation = AnimationPool.Pop();
            }

            animation.InstanceScaleFromTo(cell,from_x,from_y,to_x,to_y, duration, completed);
            return animation;
        }

        public static ScaleAnimation ScaleTo(FrameworkElement cell, double targetX, double targetY, TimeSpan duration, Action<FrameworkElement> completed)
        {
            ScaleAnimation animation = null;
            if (AnimationPool.Count == 0)
            {
                animation = new ScaleAnimation();
            }
            else
            {
                animation = AnimationPool.Pop();
            }

            animation.InstanceScaleTo(cell, targetX, targetY, duration, completed);
            return animation;
        }

        public void InstanceScaleFromTo(FrameworkElement cell,
                double from_x, double from_y,
                double to_x, double to_y,
                TimeSpan duration, Action<FrameworkElement> completed)
        {
            CompositeTransform transform = cell.RenderTransform as CompositeTransform;
            if (transform == null)
            {
                cell.RenderTransform = new CompositeTransform();
                cell.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            }

            cell.RenderTransform.SetValue(CompositeTransform.ScaleXProperty, from_x);
            cell.RenderTransform.SetValue(CompositeTransform.ScaleYProperty, from_y);
            this.InstanceScaleTo(cell, to_x, to_y, duration, completed);
        }

        public void InstanceScaleTo(FrameworkElement cell, double targetX, double targetY, TimeSpan duration, Action<FrameworkElement> completed)
        {
            this.Animate(cell, duration, targetX, targetY, completed);
        }

        private void Animate(FrameworkElement cell, TimeSpan duration, double targetX, double targetY, Action<FrameworkElement> completed)
        {
            AnimationTarget = cell;
            AnimationCompleted = completed;
            TargetX = targetX;
            TargetY = targetY;

            if (_Storyboard == null)
            {
                Init();
            }
            else
            {
                _Storyboard.Stop();
            }

            /*time*/
            _KeyFrame_x_to.KeyTime = KeyTime.FromTimeSpan(duration);
            _KeyFrame_y_to.KeyTime = KeyTime.FromTimeSpan(duration);

            /*value*/
            CompositeTransform transform = cell.RenderTransform as CompositeTransform;
            if (transform == null)
            {
                cell.RenderTransform = transform = new CompositeTransform();
                cell.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            }
            _KeyFrame_x_from.Value = transform.ScaleX;
            _KeyFrame_x_to.Value = targetX;
            _KeyFrame_y_from.Value = transform.ScaleY;
            _KeyFrame_y_to.Value = targetY;

            Storyboard.SetTarget(_Animation_X, AnimationTarget);
            Storyboard.SetTarget(_Animation_Y, AnimationTarget);

            _Storyboard.Begin();
        }

        private void _Storyboard_Completed(object sender, object e)
        {
            this.AnimationTarget.RenderTransform.SetValue(CompositeTransform.ScaleXProperty, TargetX);
            this.AnimationTarget.RenderTransform.SetValue(CompositeTransform.ScaleYProperty, TargetY);
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
