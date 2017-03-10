﻿namespace Ana.Source.Engine.OperatingSystems
{
    using Output;
    using System;
    using System.Collections.Generic;
    using Utils.Extensions;

    /// <summary>
    /// Defines an OS independent region in process memory space.
    /// </summary>
    internal class NormalizedRegion
    {
        /// <summary>
        /// The size of the region.
        /// </summary>
        private Int32 regionSize;

        /// <summary>
        /// The memory alignment of this region.
        /// </summary>
        private Int32 alignment;

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedRegion" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address of the region.</param>
        /// <param name="regionSize">The size of the region.</param>
        public NormalizedRegion(IntPtr baseAddress, Int32 regionSize)
        {
            this.BaseAddress = baseAddress;
            this.RegionSize = regionSize;
        }

        /// <summary>
        /// Gets or sets the base address of the region.
        /// </summary>
        public IntPtr BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the size of the region.
        /// </summary>
        public Int32 RegionSize
        {
            get
            {
                return this.regionSize;
            }

            set
            {
                this.regionSize = value.Clamp(0, Int32.MaxValue);
            }
        }

        /// <summary>
        /// Gets or sets the end address of the region.
        /// </summary>
        public IntPtr EndAddress
        {
            get
            {
                return this.BaseAddress.Add(this.RegionSize);
            }

            set
            {
                this.RegionSize = (Int32)value.Subtract(this.BaseAddress);
            }
        }

        /// <summary>
        /// Gets or sets the memory alignment, typically aligned with external process pointer size.
        /// </summary>
        public Int32 Alignment
        {
            get
            {
                return this.alignment;
            }

            set
            {
                this.alignment = value.Clamp(1, Int32.MaxValue);

                if (this.BaseAddress.Mod(this.alignment).ToUInt64() == 0)
                {
                    return;
                }

                // Enforce alignment constraint on base address
                unchecked
                {
                    IntPtr endAddress = this.EndAddress;
                    this.BaseAddress = this.BaseAddress.Subtract(this.BaseAddress.Mod(this.alignment), wrapAround: false);
                    this.BaseAddress = this.BaseAddress.Add(this.alignment);
                    this.EndAddress = endAddress;
                }
            }
        }

        /// <summary>
        /// Determines if a page has a higher base address.
        /// </summary>
        /// <param name="first">The first region being compared.</param>
        /// <param name="second">The second region being compared.</param>
        /// <returns>The result of the comparison.</returns>
        public static Boolean operator >(NormalizedRegion first, NormalizedRegion second)
        {
            return first.BaseAddress.ToUInt64() > second.BaseAddress.ToUInt64();
        }

        /// <summary>
        /// Determines if a page has a lower base address.
        /// </summary>
        /// <param name="first">The first region being compared.</param>
        /// <param name="second">The second region being compared.</param>
        /// <returns>The result of the comparison.</returns>
        public static Boolean operator <(NormalizedRegion first, NormalizedRegion second)
        {
            return first.BaseAddress.ToUInt64() < second.BaseAddress.ToUInt64();
        }

        /// <summary>
        /// Determines if a page has an equal or higher base address.
        /// </summary>
        /// <param name="first">The first region being compared.</param>
        /// <param name="second">The second region being compared.</param>
        /// <returns>The result of the comparison.</returns>
        public static Boolean operator >=(NormalizedRegion first, NormalizedRegion second)
        {
            return first.BaseAddress.ToUInt64() >= second.BaseAddress.ToUInt64();
        }

        /// <summary>
        /// Determines if a page has an equal or lower base address.
        /// </summary>
        /// <param name="first">The first region being compared.</param>
        /// <param name="second">The second region being compared.</param>
        /// <returns>The result of the comparison.</returns>
        public static Boolean operator <=(NormalizedRegion first, NormalizedRegion second)
        {
            return first.BaseAddress.ToUInt64() <= second.BaseAddress.ToUInt64();
        }

        /// <summary>
        /// Determines if an address is contained in this snapshot.
        /// </summary>
        /// <param name="address">The address for which to search.</param>
        /// <returns>True if the address is contained.</returns>
        public virtual Boolean ContainsAddress(IntPtr address)
        {
            if (address.ToUInt64() >= this.BaseAddress.ToUInt64() && address.ToUInt64() <= this.EndAddress.ToUInt64())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Expands a region by the element type size in both directions unconditionally.
        /// </summary>
        /// <param name="expandSize">The size by which to expand this region.</param>
        public virtual void Expand(Int32 expandSize)
        {
            this.BaseAddress = this.BaseAddress.Subtract(expandSize, wrapAround: false);
            this.RegionSize += expandSize;
        }

        /// <summary>
        /// Returns a collection of regions within this region, based on the specified chunking size.
        /// Ex) If this region is 257 bytes, chunking with a size of 64 will return 5 new regions.
        /// </summary>
        /// <param name="chunkSize">The size to break down the region into.</param>
        /// <returns>A collection of regions broken down from the original region based on the chunk size.</returns>
        public IEnumerable<NormalizedRegion> ChunkNormalizedRegion(Int32 chunkSize)
        {
            if (chunkSize <= 0)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Fatal, "Invalid chunk size specified for region");
            }

            chunkSize = Math.Min(chunkSize, this.RegionSize);

            Int32 chunkCount = (this.RegionSize / chunkSize) + (this.RegionSize % chunkSize == 0 ? 0 : 1);

            NormalizedRegion[] chunks = new NormalizedRegion[chunkCount];

            for (Int32 index = 0; index < chunkCount; index++)
            {
                Int32 size = chunkSize;

                // Set size to the remainder if on the final chunk and they are not divisible evenly
                if (index == chunkCount - 1 && this.RegionSize > chunkSize && this.RegionSize % chunkSize != 0)
                {
                    size = this.RegionSize % chunkSize;
                }

                chunks[index] = new NormalizedRegion(this.BaseAddress.Add(chunkSize * index), size);
            }

            return chunks;
        }
    }
    //// End interface
}
//// End namespace