////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017-2019 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DdsFileTypePlus
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(TextureCollectionDebugView))]
    internal sealed class TextureCollection : IList<Texture>, IReadOnlyList<Texture>, IDisposable
    {
        private readonly List<Texture> items;
        private bool disposed;

        public TextureCollection(int capacity)
        {
            this.items = new List<Texture>(capacity);
        }

        public int Capacity
        {
            get => this.items.Capacity;
            set => this.items.Capacity = value;
        }

        public int Count => this.items.Count;

        public Texture this[int index]
        {
            get
            {
                return this.items[index];
            }
            set
            {
                DisposePreviousItem(index);

                this.items[index] = value;
            }
        }

        bool ICollection<Texture>.IsReadOnly => false;

        public void Add(Texture item)
        {
            this.items.Add(item);
        }

        public void Clear()
        {
            for (int i = 0; i < this.items.Count; i++)
            {
                this.items[i]?.Dispose();
            }

            this.items.Clear();
        }

        public bool Contains(Texture item)
        {
            return this.items.Contains(item);
        }

        public void CopyTo(Texture[] array, int arrayIndex)
        {
            this.items.CopyTo(array, arrayIndex);
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;

                for (int i = 0; i < this.items.Count; i++)
                {
                    this.items[i]?.Dispose();
                }
            }
        }

        public IEnumerator<Texture> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        public int IndexOf(Texture item)
        {
            return this.items.IndexOf(item);
        }

        public void Insert(int index, Texture item)
        {
            DisposePreviousItem(index);

            this.items.Insert(index, item);
        }

        public bool Remove(Texture item)
        {
            int index = IndexOf(item);

            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            DisposePreviousItem(index);

            this.items.RemoveAt(index);
        }

        private void DisposePreviousItem(int index)
        {
            if ((uint)index < (uint)this.items.Count)
            {
                this.items[index]?.Dispose();
            }
        }

        private sealed class TextureCollectionDebugView
        {
            private readonly TextureCollection textures;

            public TextureCollectionDebugView(TextureCollection textures)
            {
                this.textures = textures;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Texture[] Items
            {
                get
                {
                    Texture[] items = new Texture[this.textures.Count];

                    this.textures.CopyTo(items, 0);

                    return items;
                }
            }

        }
    }
}
