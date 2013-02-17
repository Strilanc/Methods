using System;

namespace Methods.ExpressionHash {
    ///<summary>A stream of 32 bit integers starting at 0 and counting to a given length.</summary>
    public sealed class CountingStream : IntStream {
        private int _position;
        private readonly int _length;
        public CountingStream(int length) {
            this._length = length;
        }
        public override int Read(int[] buffer) {
            var outCount = Math.Min(_length - _position, buffer.Length);
            for (var i = 0; i < outCount; i++) {
                buffer[i] = i + _position;
            }
            _position += outCount;
            return outCount;
        }
    }
}