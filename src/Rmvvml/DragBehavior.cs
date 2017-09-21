using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Rmvvml
{
    /// <summary>
    /// データのドラッグを可能にします
    /// </summary>
    public class DragBehavior : Behavior<FrameworkElement>
    {
        #region DataType

        /// <summary>
        /// ドラッグドロップのデータの種類
        /// DragBehaviorとDropBehaviorのペアリングのために使用
        /// </summary>
        public object DataType
        {
            get { return (object)GetValue(DataTypeProperty); }
            set { SetValue(DataTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataTypeProperty =
            DependencyProperty.Register("DataType", typeof(object), typeof(DragBehavior), new PropertyMetadata(null));

        #endregion

        /// <summary>
        /// このコントロールの上でマウスボタンが押下されている状態かどうか
        /// </summary>
        bool IsMouseDownOnThis { get; set; } = false;

        /// <summary>
        /// 現在表示中のAdorner
        /// </summary>
        DragAdorner ShowingAdorner { get; set; }

        FrameworkElement GetAdornerRoot(DependencyObject d)
        {
            var ret = VisualTreeHelperExtension.Ancestors(d)
                .OfType<FrameworkElement>()
                .Reverse()
                .FirstOrDefault(x => AdornerLayer.GetAdornerLayer(x) != null);
            return ret;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseDown += OnMouseDown;
        }

        void ClearIsMouseDownOnThis()
        {
            IsMouseDownOnThis = false;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            IsMouseDownOnThis = true;
        }

        // ドラッグのほうは子のテキストボックスを優先してやらないと、テキストの選択ができなくなる
        // だからちゃんとバブルのMouseDownが来たことを確認する必要がある
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsMouseDownOnThis)
            {
                return;
            }

            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                ClearIsMouseDownOnThis();
                return;
            }

            var root = GetAdornerRoot(AssociatedObject);
            root.QueryContinueDrag += Root_QueryContinueDrag;

            ShowingAdorner = new DragAdorner(root) { DragFrom = AssociatedObject, };
            ShowingAdorner.Show();

            // とりあえずドラッグ風
            // DragDropEffects.AllにLinkが含まれないバグ回避
            var data = new DataObject();
            data.SetData(DataType?.ToString() ?? "test", AssociatedObject.DataContext);
            DragDrop.DoDragDrop(AssociatedObject, data, DragDropEffects.All | DragDropEffects.Link);

            ShowingAdorner.Hide();
            ShowingAdorner = null;

            root.QueryContinueDrag -= Root_QueryContinueDrag;
        }

        // DragOverはAllowDropなコントロールの上でしか使えない
        private void Root_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (ShowingAdorner == null) return;

            var fe = sender as FrameworkElement;
            // Mouse.GetPositionはドラッグ中値が更新されないのでWin32APIを使う必要がある
            //var point = Mouse.GetPosition(fe);
            var point = CursorInfo.GetPosition(fe);

            //System.Diagnostics.Debug.WriteLine("{0} DragBehavior.OnQueryContinueDrag ({1},{2})",
            //    DateTime.Now.ToString("HH:mm:ss.fff"),
            //    point.X,
            //    point.Y
            //    );

            ShowingAdorner.UpdateCursorPosition(point);
        }
    }

    /// <summary>
    /// マウスカーソルの位置を取得するためのクラスです。
    /// </summary>
    public class CursorInfo
    {
        [DllImport("user32.dll")]
        private static extern void GetCursorPos(out POINT pt);

        private struct POINT
        {
            public Int32 X;
            public Int32 Y;
        }

        /// <summary>
        /// 指定したコントロールに対するマウスカーソルの相対位置を返します
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Point GetPosition(System.Windows.Media.Visual v)
        {
            POINT p;
            GetCursorPos(out p);

            Point converted = v.PointFromScreen(new Point(p.X, p.Y));
            //System.Diagnostics.Debug.WriteLine("GetNowPosition PointFromScreen ({0},{1}) => ({2},{3})",
            //    p.X, p.Y, converted.X, converted.Y);
            return converted;
        }
    }


    public class DragAdorner : SimpleAdornerBase
    {
        public DragAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            Line = new Line()
            {
                Stroke = Brushes.LimeGreen,
                StrokeThickness = 5,
                StrokeDashArray = new DoubleCollection(new double[] { 3, 2, }),
            };

            DoubleAnimation animation = new DoubleAnimation();
            Storyboard.SetTarget(animation, Line);
            Storyboard.SetTargetProperty(animation, new PropertyPath("StrokeDashOffset"));
            animation.RepeatBehavior = RepeatBehavior.Forever;
            animation.Duration = TimeSpan.FromSeconds(10);
            animation.From = 200;
            animation.To = 0;

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);

            Story = storyboard;

            BeginStoryboard(Story);

            Host.Children.Add(Line);
        }

        public FrameworkElement DragFrom { get; set; }

        Line Line { get; set; }

        Storyboard Story { get; set; }

        // adornerLayerに対する相対位置
        public void UpdateCursorPosition(Point p)
        {
            var srcPoint = DragFrom.TranslatePoint(new Point(DragFrom.ActualWidth / 2, DragFrom.ActualHeight / 2), AdornedElement);

            Line.X1 = srcPoint.X;
            Line.Y1 = srcPoint.Y;
            Line.X2 = p.X;
            Line.Y2 = p.Y;

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(AdornedElement);
            layer.Update(AdornedElement);

            //System.Diagnostics.Debug.WriteLine("{0} UpdateCursorPosition ({1},{2}) => ({3},{4})",
            //    DateTime.Now.ToString("HH:mm:ss.fff"),
            //    srcPoint.X,
            //    srcPoint.Y,
            //    p.X,
            //    p.Y
            //    );
        }
    }
}
