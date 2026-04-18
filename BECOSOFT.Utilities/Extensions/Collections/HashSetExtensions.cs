using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace BECOSOFT.Utilities.Extensions.Collections {
    public static class HashSetExtensions {
        private static readonly ConcurrentDictionary<Type, HashSetTypeInfo> _hashSetTypeInfo = new ConcurrentDictionary<Type, HashSetTypeInfo>();

        private delegate bool TryGetValueFunc(object set, object equalValue, ref object actualValue);
        /// <summary>Searches the set for a given value and returns the equal value it finds, if any.</summary>
        /// <param name="set"></param>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">The value from the set that the search found, or the default value of T when the search yielded no match.</param>
        /// <returns>A value indicating whether the search was successful.</returns>
        public static bool TryGetValue<T>(this HashSet<T> set, T equalValue, out T actualValue) {
            var info = _hashSetTypeInfo.GetOrAdd(typeof(T), key => new HashSetTypeInfo(set.GetType(), typeof(T)));
            object outParam = default(T);
            var res = info.TryGetValueFunction(set, equalValue, ref outParam);
            actualValue = (T)outParam;
            return res;
        }

        private class HashSetTypeInfo : IEquatable<HashSetTypeInfo> {

            internal Type SetType { get; }
            internal Type ElementType { get; }
            internal TryGetValueFunc TryGetValueFunction { get; }
            internal HashSetTypeInfo(Type setType, Type elementType) {
                /*
                 Implementation:
                    public bool TryGetValue(T equalValue, out T actualValue)
                    {
                        if (this.m_buckets != null)
                        {
                          int index = this.InternalIndexOf(equalValue);
                          if (index >= 0)
                          {
                            actualValue = this.m_slots[index].value;
                            return true;
                          }
                        }
                        actualValue = default (T);
                        return false;
                    }   
                 */
                SetType = setType;
                ElementType = elementType;
                var internalIndexOfMethod = setType.GetMethod("InternalIndexOf", BindingFlags.NonPublic | BindingFlags.Instance);
                var bucketsField = setType.GetField("m_slots", BindingFlags.NonPublic | BindingFlags.Instance);
                var bucketElementType = bucketsField.FieldType.GetElementType();
                var bucketValueField = bucketElementType.GetField("value", BindingFlags.NonPublic | BindingFlags.Instance);

                var objectParam = Expression.Parameter(typeof(object), "objectSet");
                var equalValueObjectParm = Expression.Parameter(typeof(object), "equalValueObject");
                var setVariable = Expression.Variable(setType, "set");
                var equalValueVariable = Expression.Variable(elementType, "equalValue");
                var actualValueOutParam = Expression.Parameter(typeof(object).MakeByRefType(), "actualValue");
                var actualValueVariable = Expression.Variable(elementType, "tempActualValue");
                var field = Expression.Field(setVariable, bucketsField);
                var indexVariable = Expression.Variable(typeof(int), "index");
                var returnTarget = Expression.Label(typeof(bool));
                var indexOf = Expression.Call(setVariable, internalIndexOfMethod, equalValueVariable);
                var emptyBucketTest = Expression.NotEqual(field, Expression.Constant(null, bucketsField.FieldType));
                var indexOfAssignment = Expression.Assign(indexVariable, indexOf);
                var slotAccessExpr = Expression.ArrayIndex(field, indexVariable);
                var slotVariable = Expression.Variable(bucketElementType, "bucket");
                var indexTrueTestBlock = Expression.Block(
                    new[] { slotVariable, actualValueVariable },
                    Expression.Assign(slotVariable, slotAccessExpr),
                    Expression.Assign(actualValueVariable, Expression.Field(slotVariable, bucketValueField)),
                    Expression.Assign(actualValueOutParam, Expression.Convert(actualValueVariable, typeof(object))),
                    Expression.Return(returnTarget, Expression.Constant(true, typeof(bool)))
                );
                var indexTest = Expression.IfThen(Expression.GreaterThanOrEqual(indexVariable, Expression.Constant(0, typeof(int))),
                                                  indexTrueTestBlock);
                var exp = Expression.Block(
                    new[] { setVariable, equalValueVariable },
                    Expression.Assign(setVariable, Expression.Convert(objectParam, setType)),
                    Expression.Assign(equalValueVariable, Expression.Convert(equalValueObjectParm, elementType)),
                    Expression.IfThen(
                        emptyBucketTest,
                        Expression.Block(new[] { indexVariable },
                                         indexOfAssignment,
                                         indexTest
                        )
                    ),
                    Expression.Assign(actualValueOutParam, Expression.Convert(Expression.Default(elementType), typeof(object))),
                    Expression.Label(returnTarget, Expression.Constant(false))
                );
                var lambda = Expression.Lambda<TryGetValueFunc>(exp, objectParam, equalValueObjectParm, actualValueOutParam);
                TryGetValueFunction = lambda.Compile();
            }

            public bool Equals(HashSetTypeInfo other) {
                if (ReferenceEquals(null, other)) {
                    return false;
                }
                if (ReferenceEquals(this, other)) {
                    return true;
                }
                return SetType == other.SetType;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }
                if (ReferenceEquals(this, obj)) {
                    return true;
                }
                if (obj.GetType() != this.GetType()) {
                    return false;
                }
                return Equals((HashSetTypeInfo)obj);
            }

            public override int GetHashCode() {
                return (SetType != null ? SetType.GetHashCode() : 0);
            }
        }
    }
}