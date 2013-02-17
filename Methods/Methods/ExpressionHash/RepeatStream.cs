using System;

namespace Methods.ExpressionHash {
    ///<summary>A stream of the same 32 bit integer repeated a given number of times.</summary>
    public sealed class RepeatStream : IntStream {
        private int _position;
        private readonly int _value;
        private readonly int _length;
        public RepeatStream(int value, int length) {
            this._value = value;
            this._length = length;
        }
        public override int Read(int[] buffer) {
            var outCount = Math.Min(_length - _position, buffer.Length);
            for (var i = 0; i < outCount; i++) {
                buffer[i] = _value;
            }
            _position += outCount;
            return outCount;
        }
    }
}