//-----------------------------------------------------------------------
// <copyright file="CircularBuffer.cs" company="alexreg">
//     (c) alexreg.
//     This source is subject to the Microsoft Public License (Ms-PL).
//     Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//     All other rights reserved.
//     Project site:  http://circularbuffer.codeplex.com/
// </copyright>
//-----------------------------------------------------------------------

namespace Silverlight.Media
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Implements a circular buffer data structure.
    /// </summary>
    /// <typeparam name="T">Type of elements contained in the circular buffer.</typeparam>
    public class CircularBuffer<T>
    {
        /// <summary>
        /// Maximum number of elements allowed in the circular buffer.
        /// </summary>
        private int capacity;

        /// <summary>
        /// Current number of elements in the circular buffer.
        /// </summary>
        private int size;

        /// <summary>
        /// Current reading index.
        /// </summary>
        private int head;

        /// <summary>
        /// Current writing index.
        /// </summary>
        private int tail;

        /// <summary>
        /// Array containing the elements of the circular buffer.
        /// </summary>
        private T[] buffer;

        /// <summary>
        /// Initializes a new instance of the CircularBuffer class.
        /// </summary>
        /// <param name="capacity">Maximum number of elements allowed in the circular bufffer.</param>
        public CircularBuffer(int capacity)
            : this(capacity, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CircularBuffer class.
        /// </summary>
        /// <param name="capacity">Maximum number of elements allowed in the circular bufffer.</param>
        /// <param name="allowOverflow">true if overflow is allowed, otherwise, false.</param>
        public CircularBuffer(int capacity, bool allowOverflow)
        {
            if (capacity < 0)
            {
                throw new ArgumentException("capacity must be greater than or equal to zero.", "capacity");
            }

            this.capacity = capacity;
            this.size = 0;
            this.head = 0;
            this.tail = 0;
            this.buffer = new T[capacity];
            this.AllowOverflow = allowOverflow;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the circular buffer allows overflow.
        /// </summary>
        public bool AllowOverflow
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum number of elements allowed in the circular buffer.
        /// </summary>
        public int Capacity
        {
            get
            {
                return this.capacity;
            }

            set
            {
                if (value == this.capacity)
                {
                    return;
                }

                if (value < this.size)
                {
                    throw new ArgumentOutOfRangeException("value", "value must be greater than or equal to the buffer size.");
                }

                var dst = new T[value];
                if (this.size > 0)
                {
                    this.CopyTo(dst);
                }

                this.buffer = dst;

                this.capacity = value;
            }
        }

        /// <summary>
        /// Gets the current number of elements in the circular buffer.
        /// </summary>
        public int Size
        {
            get { return this.size; }
        }

        /// <summary>
        /// Searches the circular buffer for a particular item.
        /// </summary>
        /// <param name="item">Item for which to search.</param>
        /// <returns>true if the item is found, otherwise, false.</returns>
        public bool Contains(T item)
        {
            int bufferIndex = this.head;
            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < this.size; i++, bufferIndex++)
            {
                if (bufferIndex == this.capacity)
                {
                    bufferIndex = 0;
                }

                if (item == null && this.buffer[bufferIndex] == null)
                {
                    return true;
                }
                else if ((this.buffer[bufferIndex] != null) &&
                    comparer.Equals(this.buffer[bufferIndex], item))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Clears the circular buffer.
        /// </summary>
        public void Clear()
        {
            this.size = 0;
            this.head = 0;
            this.tail = 0;
        }

        /// <summary>
        /// Writes data to the circular buffer.
        /// </summary>
        /// <param name="src">data to write to the circular buffer.</param>
        /// <returns>Number of bytes written to the cirular buffer.</returns>
        public int Put(T[] src)
        {
            return this.Put(src, 0, src.Length);
        }

        /// <summary>
        /// Writes data to the circular buffer.
        /// </summary>
        /// <param name="src">Data to write to the circular buffer.</param>
        /// <param name="offset">A 32-bit integer that represents the index in the src at which reading begins.</param>
        /// <param name="count">Number of elements to write.</param>
        /// <returns>Number of bytes written to the cirular buffer.</returns>
        public int Put(T[] src, int offset, int count)
        {
            int realCount = this.AllowOverflow ? count : Math.Min(count, this.capacity - this.size);
            int srcIndex = offset;
            for (int i = 0; i < realCount; i++, this.tail++, srcIndex++)
            {
                if (this.tail == this.capacity)
                {
                    this.tail = 0;
                }

                this.buffer[this.tail] = src[srcIndex];
            }

            this.size = Math.Min(this.size + realCount, this.capacity);
            return realCount;
        }

        /// <summary>
        /// Writes a single element to the circular buffer.
        /// </summary>
        /// <param name="item">Item to write to the circular buffer.</param>
        public void Put(T item)
        {
            if ((!this.AllowOverflow) && (this.size == this.capacity))
            {
                throw new OverflowException("Buffer is full.");
            }

            this.buffer[this.tail] = item;
            if (this.tail++ == this.capacity)
            {
                this.tail = 0;
            }

            this.size++;
        }

        /// <summary>
        /// Advances the read pointer a specified number of elements.
        /// </summary>
        /// <param name="count">A 32-bit integer that represents the number of elements to skip.</param>
        public void Skip(int count)
        {
            this.head += count;
            if (this.head >= this.capacity)
            {
                this.head -= this.capacity;
            }
            else if (this.head < 0)
            {
                // Handle negatives
                this.head += this.capacity;
            }
        }

        /// <summary>
        /// Reads a specified number of elements from the circular buffer without advancing the current read position.
        /// </summary>
        /// <param name="count">A 32-bit integer that represents the number of elements to read.</param>
        /// <returns>An array containing the elements read.</returns>
        public T[] Peek(int count)
        {
            var dst = new T[count];
            this.Peek(dst);
            return dst;
        }

        /// <summary>
        /// Reads elements from the circular buffer without advancing the current read position.
        /// </summary>
        /// <param name="dst">Buffer to receive the elements from the circular buffer.</param>
        /// <returns>Number of bytes placed into the buffer.</returns>
        public int Peek(T[] dst)
        {
            return this.Peek(dst, 0, dst.Length);
        }

        /// <summary>
        /// Reads elements from the circular buffer without advancing the current read position.
        /// </summary>
        /// <param name="dst">Buffer to receive the elements from the circular buffer.</param>
        /// <param name="offset">A 32-bit integer that represents the index in the src at which writing begins.</param>
        /// <param name="count">Number of elements to read.</param>
        /// <returns>Number of bytes placed into the buffer.</returns>
        public int Peek(T[] dst, int offset, int count)
        {
            int realCount = Math.Min(count, this.size);
            int dstIndex = offset;
            int tempHead = this.head;
            for (int i = 0; i < realCount; i++, tempHead++, dstIndex++)
            {
                if (tempHead == this.capacity)
                {
                    tempHead = 0;
                }
                                
                dst[dstIndex] = this.buffer[tempHead];
            }

            return realCount;
        }

        /// <summary>
        /// Reads a single element from the circular buffer without advancing the current read position.
        /// </summary>
        /// <returns>Element read from the circular buffer.</returns>
        public T Peek()
        {
            if (this.size == 0)
            {
                throw new InvalidOperationException("Buffer is empty.");
            }

            int tempHead = (this.head == this.capacity) ? 0 : this.head;

            var item = this.buffer[tempHead];
            return item;
        }

        /// <summary>
        /// Reads a specified number of elements from the circular buffer and advances the current read position.
        /// </summary>
        /// <param name="count">A 32-bit integer that represents the number of elements to read.</param>
        /// <returns>An array containing the elements read.</returns>
        public T[] Get(int count)
        {
            var dst = new T[count];
            this.Get(dst);
            return dst;
        }

        /// <summary>
        /// Reads elements from the circular buffer and advances the current read position.
        /// </summary>
        /// <param name="dst">Buffer to receive the elements from the circular buffer.</param>
        /// <returns>Number of bytes placed into the buffer.</returns>
        public int Get(T[] dst)
        {
            return this.Get(dst, 0, dst.Length);
        }

        /// <summary>
        /// Reads elements from the circular buffer and advances the current read position.
        /// </summary>
        /// <param name="dst">Buffer to receive the elements from the circular buffer.</param>
        /// <param name="offset">A 32-bit integer that represents the index in the src at which writing begins.</param>
        /// <param name="count">Number of elements to read.</param>
        /// <returns>Number of bytes placed into the buffer.</returns>
        public int Get(T[] dst, int offset, int count)
        {
            int realCount = Math.Min(count, this.size);
            int dstIndex = offset;
            for (int i = 0; i < realCount; i++, this.head++, dstIndex++)
            {
                if (this.head == this.capacity)
                {
                    this.head = 0;
                }

                dst[dstIndex] = this.buffer[this.head];
            }

            this.size -= realCount;
            return realCount;
        }

        /// <summary>
        /// Reads a single element from the circular buffer and advances the current read position.
        /// </summary>
        /// <returns>Element read from the circular buffer.</returns>
        public T Get()
        {
            if (this.size == 0)
            {
                throw new InvalidOperationException("Buffer is empty.");
            }

            // Missing check for when size != 0 and one of the other get methods is called.  It leaves the head pointer == capacity.
            if (this.head == this.capacity)
            {
                this.head = 0;
            }

            var item = this.buffer[this.head];

            // We probably don't need this now, as we are checking BEFORE we read, like the other methods.
            if (this.head++ == this.capacity)
            {
                this.head = 0;
            }

            this.size--;
            return item;
        }

        /// <summary>
        /// Copies the elements from the circular buffer to an array.
        /// </summary>
        /// <param name="array">Destination array.</param>
        public void CopyTo(T[] array)
        {
            this.CopyTo(array, 0);
        }

        /// <summary>
        /// Copies the elements from the circular buffer to an array.
        /// </summary>
        /// <param name="array">Destination array.</param>
        /// <param name="arrayIndex">A 32-bit integer that represents the index in the array at which writing begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            // I'm thinking this is just wrong.  We should be using the array.Length.
            this.CopyTo(array, arrayIndex, this.size);
        }

        /// <summary>
        /// Copies the elements from the circular buffer to an array.
        /// </summary>
        /// <param name="array">Destination array.</param>
        /// <param name="arrayIndex">A 32-bit integer that represents the index in the array at which writing begins.</param>
        /// <param name="count">Number of elements to copy.</param>
        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            if (count > this.size)
            {
                throw new ArgumentOutOfRangeException("count", "count cannot be greater than the buffer size.");
            }

            int bufferIndex = this.head;
            for (int i = 0; i < count; i++, bufferIndex++, arrayIndex++)
            {
                if (bufferIndex == this.capacity)
                {
                    bufferIndex = 0;
                }

                array[arrayIndex] = this.buffer[bufferIndex];
            }
        }
    }
}
