using System;
using System.Linq;
using System.Linq.Expressions;

namespace Methods.ExpressionHash {
    ///<summary>Runtime description of a simple hashing algorithm.</summary>
    public struct DynamicHasher {
        ///<summary>A type of mathematical operation.</summary>
        public enum Operation {
            Add,
            Multiply,
            Subtract,
            Xor,
            Or,
            And
        }
        ///<summary>A hashing step, describing how to feed the current state through a mathematical operation.</summary>
        public struct Step {
            public int LeftInputIndex;
            public int RightInputIndex;
            public int OutputIndex;
            public Operation Operation;
        }

        ///<summary>
        ///The initial state of the hash function, before any data is processed.
        ///Also implicitly determines the size of the state (which input/output indexes are valid).
        ///Index 0 is always overwritten with the current input.
        ///The output of the hash function is last state value once all data has been processed.
        ///</summary>
        public int[] InitialState;
        ///<summary>The steps to apply to the current state for each value in a data stream.</summary>
        public Step[] Steps;

        ///<summary>The result of applying this hash function to the given data, by interpreting the operations in the inner loop.</summary>
        public int Interpret(IntStream data) {
            // copy initial state
            var state = InitialState.ToArray();

            var buffer = new int[1 << 12];
            while (true) {
                var n = data.Read(buffer);
                if (n == 0) break;

                // feed data through the state via custom steps
                for (var i = 0; i < n; i++) {
                    // input into hash via first state variable
                    state[0] = buffer[i];

                    // apply custom hashing steps
                    foreach (var step in Steps) {
                        // read
                        var lhs = state[step.LeftInputIndex];
                        var rhs = state[step.RightInputIndex];

                        // eval
                        int result;
                        switch (step.Operation) {
                        case Operation.Add:
                            unchecked {
                                result = lhs + rhs;
                            }
                            break;
                        case Operation.Multiply:
                            unchecked {
                                result = lhs*rhs;
                            }
                            break;
                        case Operation.Subtract:
                            unchecked {
                                result = lhs - rhs;
                            }
                            break;
                        case Operation.Xor:
                            result = lhs ^ rhs;
                            break;
                        case Operation.Or:
                            result = lhs | rhs;
                            break;
                        case Operation.And:
                            result = lhs & rhs;
                            break;
                        default:
                            throw new InvalidOperationException();
                        }

                        // write
                        state[step.OutputIndex] = result;
                    }
                }
            }
            // result is last state variable
            return state[state.Length - 1];
        }

        ///<summary>A compilable expression containing specialized code for applying this hash function.</summary>
        public Expression<Func<IntStream, int>> Specialize() {
            var dataParam = Expression.Parameter(typeof (IntStream));
            var readCountVar = Expression.Variable(typeof (int));
            var bufferVar = Expression.Variable(typeof (int[]));
            var stateVars = InitialState.Select(_ => Expression.Variable(typeof (int))).ToArray();

            // combine all hashing steps into a single unconditional block of statements
            var runAllStepsBlock = Expression.Block(Steps.Select(step => {
                var lhs = stateVars[step.LeftInputIndex];
                var rhs = stateVars[step.RightInputIndex];
                Expression result;
                switch (step.Operation) {
                case Operation.Add:
                    result = Expression.Add(lhs, rhs);
                    break;
                case Operation.Multiply:
                    result = Expression.Multiply(lhs, rhs);
                    break;
                case Operation.Subtract:
                    result = Expression.Subtract(lhs, rhs);
                    break;
                case Operation.Xor:
                    result = Expression.ExclusiveOr(lhs, rhs);
                    break;
                case Operation.Or:
                    result = Expression.Or(lhs, rhs);
                    break;
                case Operation.And:
                    result = Expression.And(lhs, rhs);
                    break;
                default:
                    throw new InvalidOperationException();
                }
                return Expression.Assign(stateVars[step.OutputIndex], result);
            }));

            // applies the hashing steps to each integer read into the buffer
            var breakTarget = Expression.Label();
            var iVar = Expression.Variable(typeof (int));
            var processBufferLoop =
                Expression.Block(
                    // declare loop variable
                    new[] {iVar},
                    // for i < readCount
                    Expression.Assign(iVar, Expression.Constant(0)),
                    Expression.Loop(
                        Expression.Block(
                            Expression.IfThen(
                                Expression.GreaterThanOrEqual(iVar, readCountVar),
                                Expression.Break(breakTarget)),
                            // input into hash via first state variable
                            Expression.Assign(stateVars[0], Expression.ArrayAccess(bufferVar, Expression.PostIncrementAssign(iVar))),
                            // run hash steps
                            runAllStepsBlock),
                        breakTarget));

            // reads integers into the buffer, until there's none left, and processing
            var bt2 = Expression.Label();
            var processDataLoop =
                Expression.Loop(
                    Expression.Block(
                        Expression.Assign(readCountVar, Expression.Call(dataParam, typeof (IntStream).GetMethod("Read"), bufferVar)),
                        Expression.IfThen(Expression.Equal(readCountVar, Expression.Constant(0)), Expression.Break(bt2)),
                        processBufferLoop),
                    bt2);

            // put it all together
            var body = Expression.Block(
                // declare variables
                new[] {bufferVar, readCountVar}.Concat(stateVars),
                // initialize state variables
                Expression.Block(InitialState.Zip(stateVars, (initialValue, stateVar) => Expression.Assign(stateVar, Expression.Constant(initialValue)))),
                // create buffer
                Expression.Assign(
                    bufferVar,
                    Expression.NewArrayInit(typeof (int), Expression.Constant(1 << 12))),
                // process
                processDataLoop,
                // result is last state variable
                stateVars.Last());

            return Expression.Lambda<Func<IntStream, int>>(body, dataParam);
        }
    }
}
