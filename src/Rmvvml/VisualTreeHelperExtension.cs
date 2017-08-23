using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Rmvvml
{
    /// <summary>
    /// VisualTreeHelperの機能拡張
    /// </summary>
    public static class VisualTreeHelperExtension
    {
        /// <summary>
        /// 引数要素とその祖先要素を列挙します
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> AncestorsOrSelf(DependencyObject obj)
        {
            var current = obj;
            while (current != null)
            {
                yield return current;
                current = VisualTreeHelper.GetParent(current);
            }
        }

        /// <summary>
        /// 引数要素の祖先要素を列挙します(引数要素は含みません)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> Ancestors(DependencyObject obj)
        {
            return Ancestors(obj).Skip(1);
        }

        /// <summary>
        /// 引数要素とその子孫要素を列挙します(幅優先)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> DescendantsOrSelf(DependencyObject obj)
        {
            Queue<DependencyObject> queue = new Queue<DependencyObject>();
            queue.Enqueue(obj);

            while (queue.Count > 0)
            {
                var target = queue.Dequeue();
                yield return target;

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(target); ++i)
                {
                    var child = VisualTreeHelper.GetChild(target, i);
                    queue.Enqueue(child);
                }
            }
        }

        /// <summary>
        /// 引数要素の子孫要素を列挙します(幅優先)(引数要素は含みません)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> Descendants(DependencyObject obj)
        {
            return DescendantsOrSelf(obj).Skip(1);
        }
    }
}
