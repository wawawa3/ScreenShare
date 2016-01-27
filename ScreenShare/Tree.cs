using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenShare
{
    /// <summary>
    /// Dictionaryクラスのコレクションを用いて、木構造を表現します。
    /// </summary>
    /// <typeparam name="TValue">キーに対する値</typeparam>
    class Tree<TValue> : Dictionary<int, TValue>
    {
        /// <summary>
        /// 枝の分岐数を設定、取得します。
        /// </summary>
        public int BranchCount { get; set; }

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        /// <param name="branchCount">枝の数</param>
        public Tree(int branchCount = 2) : base() 
        {
            BranchCount = branchCount;
        }

        /// <summary>
        /// キーに対応するノードの親のキーを取得します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>キーに対応するノードの親のキー</returns>
        public int GetParentKey(int key)
        {
            var k = (key - 1) / BranchCount;

            if (!this.ContainsKey(k))
                throw new ArgumentOutOfRangeException();

            return k;
        }


        /// <summary>
        /// コレクションの最後のキーを取得します。
        /// </summary>
        /// <returns>コレクションの最後のキー</returns>
        public int GetLastKey()
        {
            return this.Last().Key;
        }
    
        /// <summary>
        /// キーに対応するノードの子のキーを取得します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>キーに対応するノードの子のキー</returns>
        public int[] GetChildrenKey(int key)
        {
            var keys = new List<int>();

            for (int i = 0; i < BranchCount; i++)
            {
                var k = (key + 1) * BranchCount - i;
                if (this.ContainsKey(k))
                    keys.Add(k);
            }

            return keys.ToArray();
        }

        /// <summary>
        /// キーに対応するノードの子を取得します。
        /// </summary>
        /// <param name="keys">キー</param>
        /// <returns>キーに対応するノードの子</returns>
        public TValue[] GetValues(int[] keys)
        {
            var values = new List<TValue>();

            foreach (var key in keys)
            {
                if (!this.ContainsKey(key))
                    throw new ArgumentOutOfRangeException();

                values.Add(this[key]);
            }

            return values.ToArray();
        }

        /// <summary>
        /// コレクション内に一致する値が存在した場合、そのキーを返します。
        /// </summary>
        /// <param name="val">値</param>
        /// <returns>一致した値のキー</returns>
        public int TryGetKey(TValue val)
        {
            foreach (var pair in this)
            {
                if (pair.Value.Equals(val))
                {
                    return pair.Key;
                }
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}
