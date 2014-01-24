using System;
using System.Collections.Generic;
using Windows.Foundation;

using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace Utility.Animations
{
    public class VibrateAnimation : AnimationBase
    {
        #region Constructor

        public VibrateAnimation()
        {
            Init();
        }

        #endregion

        #region Properties

        private DoubleAnimationUsingKeyFrames _Animation_X = null;

        private EasingDoubleKeyFrame _KeyFrame_x_1 = null;
        private EasingDoubleKeyFrame _KeyFrame_x_2 = null;
        private EasingDoubleKeyFrame _KeyFrame_x_3 = null;
        private EasingDoubleKeyFrame _KeyFrame_x_4 = null;
        private EasingDoubleKeyFrame _KeyFrame_x_5 = null;
        private EasingDoubleKeyFrame _KeyFrame_x_6 = null;

        private static Stack<VibrateAnimation> AnimationPool = new Stack<VibrateAnimation>();

        #endregion

        #region Animation

        private void Init()
        {
            _Storyboard = new Storyboard();
            _Storyboard.Completed += _Storyboard_Completed;

            /*** animation x ***/
            _Animation_X = new DoubleAnimationUsingKeyFrames();

            /* key frame x 1 */
            _KeyFrame_x_1 = new EasingDoubleKeyFrame();
            _KeyFrame_x_1.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
            _KeyFrame_x_1.Value = 0;
            _Animation_X.KeyFrames.Add(_KeyFrame_x_1);

            /* key frame x 2 */
            _KeyFrame_x_2 = new EasingDoubleKeyFrame();
            _KeyFrame_x_2.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.04));
            _KeyFrame_x_2.Value = 15;
            _Animation_X.KeyFrames.Add(_KeyFrame_x_2);

            /* key frame x 3 */
            _KeyFrame_x_3 = new EasingDoubleKeyFrame();
            _KeyFrame_x_3.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.12));
            _KeyFrame_x_3.Value = -15;
            _Animation_X.KeyFrames.Add(_KeyFrame_x_3);

            /* key frame x 4 */
            _KeyFrame_x_4 = new EasingDoubleKeyFrame();
            _KeyFrame_x_4.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.20));
            _KeyFrame_x_4.Value = 15;
            _Animation_X.KeyFrames.Add(_KeyFrame_x_4);

            /* key frame x 5 */
            _KeyFrame_x_5 = new EasingDoubleKeyFrame();
            _KeyFrame_x_5.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.28));
            _KeyFrame_x_5.Value = -15;
            _Animation_X.KeyFrames.Add(_KeyFrame_x_5);

            /* key frame x 6 */
            _KeyFrame_x_6 = new EasingDoubleKeyFrame();
            _KeyFrame_x_6.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.32));
            _KeyFrame_x_6.Value = 0;
            _Animation_X.KeyFrames.Add(_KeyFrame_x_6);

            Storyboard.SetTargetProperty(_Animation_X,new PropertyPath( "(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            _Storyboard.Children.Add(_Animation_X);
        }

        public static VibrateAnimation Vibrate(FrameworkElement cell, Action<FrameworkElement> completed)
        {
            VibrateAnimation animation = null;
            if (AnimationPool.Count == 0)
            {
                animation = new VibrateAnimation();
            }
            else
            {
                animation = AnimationPool.Pop();
            }
            animation.InstanceVibrate(cell, completed);
            return animation;
        }

        public void InstanceVibrate(FrameworkElement cell, Action<FrameworkElement> completed)
        {
            this.Animate(cell, completed);
        }

        private void Animate(FrameworkElement cell, Action<FrameworkElement> completed)
        {
            AnimationTarget = cell;
            AnimationCompleted = completed;

            CompositeTransform transform = cell.RenderTransform as CompositeTransform;
            if (transform==null)
            {
                cell.RenderTransform = new CompositeTransform();
                cell.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            }

            if (_Storyboard == null)
            {
                Init();
            }
            else
            {
                _Storyboard.Stop();
            }

            Storyboard.SetTarget(_Animation_X, AnimationTarget);

            _Storyboard.Begin();
        }

        private void _Storyboard_Completed(object sender, object e)
        {
            AnimationTarget.RenderTransform.SetValue(CompositeTransform.TranslateXProperty, 0d);
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
