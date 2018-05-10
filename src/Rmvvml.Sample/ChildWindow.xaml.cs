using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rmvvml.Sample
{
    /// <summary>
    /// ChildWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ChildWindow : Window
    {
        public ChildWindow()
        {
            InitializeComponent();
        }
    }



    public class MyMultiSelecter : MultiSelector
    {
        public MyMultiSelecter()
        {
            CanSelectMultipleItems = true;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MyMultiSelecterItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MyMultiSelecterItem;
        }

        public void NotifyItemMouseDown(MyMultiSelecterItem selected)
        {
            if (IsUpdatingSelectedItems) return;

            var oldSelectedVMs = SelectedItems.OfType<object>().ToList();
            var vm = ItemContainerGenerator.ItemFromContainer(selected);

            BeginUpdateSelectedItems();

            SelectedItems.Clear();
            SelectedItems.Add(vm);
            foreach(var oldSelectedVM in oldSelectedVMs)
            {
                var oldSelected = ItemContainerGenerator.ContainerFromItem(oldSelectedVM) as MyMultiSelecterItem;
                if(oldSelected != null)
                {
                    oldSelected.IsSelected = false;
                }
            }
            selected.IsSelected = true;

            EndUpdateSelectedItems();
        }

        RectAdorner adorner;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            adorner = new RectAdorner(this);
            adorner.Origin = e.GetPosition(this);
            adorner.Show();
            Mouse.Capture(this);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (Mouse.Captured == null) return;
            adorner.Destination = e.GetPosition(this);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            if (Mouse.Captured == null) return;
            Mouse.Capture(null);
            adorner.Hide();
            var origin = adorner.Origin;
            var dest = adorner.Destination;
            adorner = null;

            List<MyMultiSelecterItem> hitItems = new List<MyMultiSelecterItem>();
            VisualTreeHelper.HitTest(
                this,
                new HitTestFilterCallback((d) =>
                {
                    if(d is MyMultiSelecterItem)
                    {
                        return HitTestFilterBehavior.ContinueSkipChildren;
                    }
                    else
                    {
                        return HitTestFilterBehavior.ContinueSkipSelf;
                    }
                }),
                new HitTestResultCallback((result) =>
                {
                    var geometryResult = result as GeometryHitTestResult;
                    if (geometryResult == null) return HitTestResultBehavior.Continue;

                    if(geometryResult.IntersectionDetail != IntersectionDetail.Empty)
                    {
                        hitItems.Add(geometryResult.VisualHit as MyMultiSelecterItem);
                    }
                    return HitTestResultBehavior.Continue;
                }),
                new GeometryHitTestParameters(new RectangleGeometry(new Rect(origin, dest))));

            var oldSelectedVMs = SelectedItems.OfType<object>().ToList();

            BeginUpdateSelectedItems();
            SelectedItems.Clear();
            foreach(var item in hitItems)
            {
                SelectedItems.Add(ItemContainerGenerator.ItemFromContainer(item));
            }
            foreach(var oldSelectedVM in oldSelectedVMs)
            {
                var oldSelected = ItemContainerGenerator.ContainerFromItem(oldSelectedVM) as MyMultiSelecterItem;
                if(oldSelected != null)
                {
                    oldSelected.IsSelected = false;
                }
            }
            foreach(var selected in hitItems)
            {
                selected.IsSelected = true;
            }
            EndUpdateSelectedItems();
        }
    }


    class RectAdorner : SimpleAdornerBase
    {
        FrameworkElement Inner { get; set; }

        public RectAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            //var innerGrid = new Grid
            //{
            //};
            //innerGrid.Children.Add(new Border
            //{
            //    BorderBrush = Brushes.Aqua,
            //    BorderThickness = new Thickness(3),
            //});
            //Inner = innerGrid;
            //Host.Children.Add(Inner);

            Host.HorizontalAlignment = HorizontalAlignment.Left;
            Host.VerticalAlignment = VerticalAlignment.Top;
            Host.Children.Add(new Border
            {
                BorderBrush = Brushes.Aqua,
                BorderThickness = new Thickness(3),
            });
        }

        Point _Origin = new Point(double.NaN, double.NaN);
        public Point Origin
        {
            get { return _Origin; }
            set
            {
                _Origin = value;
                OnPositionChanged();
            }
        }

        Point _Destination = new Point(double.NaN, double.NaN);
        public Point Destination
        {
            get { return _Destination; }
            set
            {
                _Destination = value;
                OnPositionChanged();
            }
        }

        void OnPositionChanged()
        {
            if (double.IsNaN(Origin.X + Origin.Y + Destination.X + Destination.Y)) return;

            var originX = Math.Max(0, Math.Min(Origin.X, Destination.X));
            var originY = Math.Max(0, Math.Min(Origin.Y, Destination.Y));
            var destX = Math.Max(0, Math.Max(Origin.X, Destination.X));
            var destY = Math.Max(0, Math.Max(Origin.Y, Destination.Y));

            Host.Margin = new Thickness(originX, originY, 0, 0);
            Host.Width = destX - originX;
            Host.Height = destY - originY;

            Update();
            //System.Diagnostics.Debug.WriteLine("PositionChange ({0},{1},{2},{3})", originX, originY, Host.Width, Host.Height);
        }
    }

    public class MyMultiSelecterItem : ContentControl
    {
        #region IsSelected
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(MyMultiSelecterItem), new PropertyMetadata(false, OnIsSelectedChanged));

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as MyMultiSelecterItem;
            if (obj == null) return;
            obj.OnIsSelectedChanged(e);
        }

        private void OnIsSelectedChanged(DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var selecter = VisualTreeHelperExtension.Ancestors(this).OfType<MyMultiSelecter>().First();
            selecter.NotifyItemMouseDown(this);
            
        }

        protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
        {
            var geo = new RectangleGeometry(VisualTreeHelper.GetDescendantBounds(this));
            return new GeometryHitTestResult(this, geo.FillContainsWithDetail(hitTestParameters.HitGeometry));
        }
    }
}
