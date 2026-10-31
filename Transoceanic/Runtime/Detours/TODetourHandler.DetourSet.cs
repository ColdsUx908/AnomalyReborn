using System.Collections;
using MonoMod.RuntimeDetour;

namespace Transoceanic.Runtime.Detours;

/// <summary>
/// 集中登记和管理运行时方法 detour。
/// </summary>
public sealed partial class TODetourHandler
{
    /// <summary>
    /// 按目标类型和方法管理钩子；删除时使用快照，失败项保留以供重试。
    /// </summary>
    internal sealed class DetourSet : IEnumerable<Hook>
    {
        private readonly Dictionary<Type, Dictionary<MethodBase, List<Hook>>> _Data = [];

        /// <summary>
        /// 按源方法及声明类型登记钩子，同一对象不重复加入；不在此安装或重新应用钩子。
        /// </summary>
        public void Add(Hook hook)
        {
            ArgumentNullException.ThrowIfNull(hook);
            Type targetType = hook.Source.DeclaringType ?? throw new ArgumentException("Hooks need a declaring type.", nameof(hook));
            if (!_Data.TryGetValue(targetType, out Dictionary<MethodBase, List<Hook>> methods))
                _Data[targetType] = methods = [];
            if (!methods.TryGetValue(hook.Source, out List<Hook> hooks))
                methods[hook.Source] = hooks = [];
            if (!hooks.Contains(hook))
                hooks.Add(hook);
        }

        /// <summary>
        /// 撤销并移除已登记钩子，成功返回 true；不存在返回 false，Undo 失败时保留登记。
        /// </summary>
        public bool Remove(Hook hook)
        {
            ArgumentNullException.ThrowIfNull(hook);
            Type type = hook.Source.DeclaringType;
            if (type is null || !_Data.TryGetValue(type, out Dictionary<MethodBase, List<Hook>> methods)
                || !methods.TryGetValue(hook.Source, out List<Hook> hooks) || !hooks.Contains(hook))
                return false;
            hook.Undo();
            hooks.Remove(hook);
            if (hooks.Count == 0)
                methods.Remove(hook.Source);
            if (methods.Count == 0)
                _Data.Remove(type);
            return true;
        }

        /// <summary>
        /// 按快照逐个尝试撤销并移除匹配的钩子；处理完毕后聚合抛出条件或撤销异常。
        /// </summary>
        public void RemoveAll(Func<Hook, bool> match)
        {
            ArgumentNullException.ThrowIfNull(match);
            Hook[] snapshot = this.ToArray();
            List<Exception> failures = [];
            foreach (Hook hook in snapshot)
            {
                try
                {
                    if (match(hook))
                        Remove(hook);
                }
                catch (Exception exception)
                {
                    failures.Add(exception);
                }
            }
            if (failures.Count > 0)
                throw new AggregateException("Detour removal failed.", failures);
        }

        /// <summary>
        /// 尝试撤销并移除全部钩子；失败项保留登记，全部处理后聚合抛出异常。
        /// </summary>
        public void Clear() => RemoveAll(_ => true);

        /// <summary>
        /// 返回快照，外部不能绕过撤销逻辑修改内部列表。
        /// </summary>
        public bool TryGetHooks(MethodBase targetMethod, out IReadOnlyList<Hook> hooks)
        {
            ArgumentNullException.ThrowIfNull(targetMethod);
            Type type = targetMethod.DeclaringType;
            if (type is not null && _Data.TryGetValue(type, out Dictionary<MethodBase, List<Hook>> methods)
                && methods.TryGetValue(targetMethod, out List<Hook> found) && found.Count > 0)
            {
                hooks = Array.AsReadOnly(found.ToArray());
                return true;
            }
            hooks = null;
            return false;
        }

        /// <summary>
        /// 返回钩子集合的快照枚举器；之后修改集合不会改变本次枚举。
        /// </summary>
        public IEnumerator<Hook> GetEnumerator() => _Data.Values.SelectMany(methods => methods.Values).SelectMany(hooks => hooks).ToArray().AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
