using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rmvvml
{
    /// <summary>
    /// IEnumerableのLinq拡張
    /// </summary>
    public static class IEnumerableExtension
    {
        /// <summary>
        /// 条件が成立する要素とそれより前の要素を返します
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        public static IEnumerable<T> TakeUntilAndIncluding<T>(this IEnumerable<T> v, Predicate<T> pred)
        {
            foreach (var item in v)
            {
                yield return item;

                if (pred.Invoke(item))
                {
                    yield break;
                }
            }
        }

        /// <summary>
        /// 条件が成立する要素より前の要素を返します
        /// 条件が成立した要素は含みません
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        public static IEnumerable<T> TakeUntilButNotIncluding<T>(this IEnumerable<T> v, Predicate<T> pred)
        {
            foreach (var item in v)
            {
                if (pred.Invoke(item))
                {
                    yield break;
                }

                yield return item;
            }
        }
    }
}
