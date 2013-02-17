namespace Methods.ExpressionHash {
    ///<summary>A stream of 32 bit integers.</summary>
    public abstract class IntStream {
        /// <summary>
        /// Streams data into the given buffer, starting at offset 0, until there is no more data or no more room.
        /// Returns the number of values written to the buffer.
        /// Returning 0 indicates the end of the stream.
        /// </summary>
        public abstract int Read(int[] buffer);
    }
}