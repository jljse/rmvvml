using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rmvvml
{
    /// <summary>
    /// プロパティが依存する他のプロパティを指定します
    /// PropertyChangedが連動して発生するようになります
    /// </summary>
    public class DependsOnAttribute : Attribute
    {
        public string PropertyName;

        public DependsOnAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }

    /// <summary>
    /// コマンド実行可否が依存する他のプロパティを指定します
    /// CanExecuteChangedが連動して発生するようになります
    /// </summary>
    public class CanExecuteDependsOnAttribute : Attribute
    {
        public string PropertyName;

        public CanExecuteDependsOnAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }

    /// <summary>
    /// プロパティ間の依存関係の情報
    /// </summary>
    class WirePropertyChangedInfo
    {
        // 普通のプロパティの依存関係
        Dictionary<string, List<string>> Dependencies { get; } = new Dictionary<string, List<string>>();
        // コマンド実行可否の依存関係
        Dictionary<string, List<string>> CanExeDependencies { get; } = new Dictionary<string, List<string>>();

        public void MakeCacheFor(Type type)
        {
            foreach (var prop in type.GetProperties())
            {
                foreach (var attr in prop.GetCustomAttributes(typeof(DependsOnAttribute), true).OfType<DependsOnAttribute>())
                {
                    if (!Dependencies.ContainsKey(attr.PropertyName))
                    {
                        Dependencies[attr.PropertyName] = new List<string>();
                    }
                    Dependencies[attr.PropertyName].Add(prop.Name);
                }
            }

            foreach (var prop in type.GetProperties())
            {
                foreach (var attr in prop.GetCustomAttributes(typeof(CanExecuteDependsOnAttribute), true).OfType<CanExecuteDependsOnAttribute>())
                {
                    if (!prop.PropertyType.IsAssignableFrom(typeof(RelayCommand)))
                    {
                        // 型がおかしい
                        throw new ArgumentException("CanExecuteDependsOn must be on RelayCommand property");
                    }

                    if (!CanExeDependencies.ContainsKey(attr.PropertyName))
                    {
                        CanExeDependencies[attr.PropertyName] = new List<string>();
                    }
                    CanExeDependencies[attr.PropertyName].Add(prop.Name);
                }
            }
        }

        public bool IsEmpty()
        {
            return Dependencies.Count == 0 && CanExeDependencies.Count == 0;
        }

        // 依存関係に従ってプロパティ変更通知を発行する
        public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = sender as ViewModelBase;

            // プロパティ値が変わった場合、そのプロパティに依存している他のプロパティに対しても変更通知を発生させる
            if (Dependencies.ContainsKey(e.PropertyName))
            {
                foreach (var propName in Dependencies[e.PropertyName])
                {
                    vm.RaisePropertyChanged(propName);
                }
            }

            // プロパティ値が変わった場合、そのプロパティに依存しているコマンド実行可否の変更通知を発生させる
            if (CanExeDependencies.ContainsKey(e.PropertyName))
            {
                foreach (var propName in CanExeDependencies[e.PropertyName])
                {
                    var command = vm.GetType().GetProperty(propName).GetValue(vm) as RelayCommand;
                    command.RaiseCanExecuteChanged();
                }
            }
        }
    }

    /// <summary>
    /// MVVM LightのViewModel基底クラス拡張
    /// </summary>
    public static class ViewModelBaseExtension
    {
        // 依存関係の情報 VMの型ごとにキャッシュ
        static Dictionary<Type, WirePropertyChangedInfo> WirePropertyChangedCache { get; } = new Dictionary<Type, WirePropertyChangedInfo>();

        /// <summary>
        /// プロパティに属性[DependsOn("XXX")]を指定すると、XXXが変更された際に連動してPropertyChangedイベントが発生します
        /// コマンドに属性[CanExecuteDependsOn("XXX")]を指定すると、XXXが変更された際に連動してCanExecuteChangedイベントが発生します
        /// </summary>
        /// <param name="vm"></param>
        public static void WirePropertyChanged(ViewModelBase vm)
        {
            if (vm == null) return;

            var type = vm.GetType();
            if (!WirePropertyChangedCache.ContainsKey(type))
            {
                var cache = new WirePropertyChangedInfo();
                cache.MakeCacheFor(type);
                WirePropertyChangedCache[type] = cache;
            }

            var deps = WirePropertyChangedCache[type];
            if(!deps.IsEmpty())
            {
                vm.PropertyChanged += deps.OnPropertyChanged;
            }
        }

    }
}
