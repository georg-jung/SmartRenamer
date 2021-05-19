using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SmartRenamer.Core.Parser;

namespace SmartRenamer.Core.Interpreter
{
    internal class SymbolEvaluator : ISymbolEvaluator
    {
        private readonly ICompositePropertyProvider _propertyProvider;

        public SymbolEvaluator(ICompositePropertyProvider propertyProvider)
        {
            _propertyProvider = propertyProvider;
        }

        public async Task<object?> Evaluate(string filePath, SymbolExpression expr, IExpressionVisitor expressionVisitor)
        {
            object? prop;
            IEnumerable<ISymbolExpressionPart> rest = expr.Rest;
            if ((expr.First, expr.Rest?.FirstOrDefault()) is (Identifier id1, Identifier id2) &&
                _propertyProvider.IsNamespaceKnown(id1.Value))
            {
                prop = await _propertyProvider.GetValue(filePath, id1.Value, id2.Value).ConfigureAwait(false);
                rest = expr.Rest.Skip(1);
            }
            else
            {
                prop = await _propertyProvider.GetValue(filePath, expr.First.Value).ConfigureAwait(false);
            }

            var pairwise = ((IEnumerable<ISymbolExpressionPart?>)rest)
                .Append(null)
                .Pairwise();
            var handledInIterationBefore = false;
            foreach (var (current, next) in pairwise)
            {
                if (handledInIterationBefore)
                {
                    handledInIterationBefore = false;
                    continue;
                }

                if (prop == null)
                    throw new NullReferenceException();

                (handledInIterationBefore, prop) = (current, next) switch
                {
                    (Indexer idx, _) => (false, await GetIndexed(prop, idx.Indices, expressionVisitor, filePath).ConfigureAwait(false)),
                    (Identifier id, Indexer idx) => await GetMemberValue(prop, id.Value, idx.Indices, expressionVisitor, filePath).ConfigureAwait(false),
                    (Identifier id, _) => (false, GetMemberValue(prop, id.Value)),
                    _ => throw new NotImplementedException(),
                };
            }
            return prop;
        }

        private static object GetMemberValue(object src, string member)
        {
            return src.GetType().GetMember(member).FirstOrDefault() switch
            {
                FieldInfo fi => fi.GetValue(src),
                PropertyInfo pi => pi.GetValue(src, null),
                _ => throw new ArgumentException($"This object does not have a public field or property member named {member}.")
            };
        }

        private static Task<(bool nextHandled, object value)> GetMemberValue(object src, string member, IReadOnlyList<Expression> idxArguments, IExpressionVisitor expressionVisitor, string filePath)
        {
            var idxProps = src.GetType().GetMember(member)
                .OfType<PropertyInfo>()
                .Where(p => p.GetIndexParameters().Length == idxArguments.Count)
                .ToList();

            if (idxProps.Count == 0)
            {
                return Task.FromResult((false, GetMemberValue(src, member)));
            }

            return GetMemberValue(src, idxProps, idxArguments, expressionVisitor, filePath);
        }

        private static async Task<(bool nextHandled, object value)> GetMemberValue(object src, List<PropertyInfo> possibleIdxProps, IReadOnlyList<Expression> idxArguments, IExpressionVisitor expressionVisitor, string filePath)
        {
            var args = await idxArguments.ToAsyncEnumerable().SelectAwait(async x => await expressionVisitor.Visit(x, filePath)).ToArrayAsync().ConfigureAwait(false);
            return GetMemberValue(src, possibleIdxProps, args);
        }

        private static (bool nextHandled, object value) GetMemberValue(object src, List<PropertyInfo> possibleIdxProps, object?[] idxArgs)
        {            
            foreach (var idxProp in possibleIdxProps)
            {
                var fits = idxProp.GetIndexParameters()
                    .Select(pi => pi.ParameterType)
                    .Zip(idxArgs, (typeProp, arg) => (typeProp, arg))
                    .All(x => x.arg == null || x.typeProp.IsAssignableFrom(x.arg.GetType()));
                if (fits)
                {
                    return (true, idxProp.GetValue(src, idxArgs));
                }
            }
            throw new ArgumentException($"While the given object has a candidate indexed property with {idxArgs.Length} index arguments, " +
                $"none of them matched to the given argument's types ({string.Join(", ", idxArgs.Select(a => a?.GetType()?.Name ?? "null"))}).");
        }

        private static object GetDefaultIndexedPropertyValue(object src, object?[] idxArgs)
        {
            var idxProps = src.GetType().GetDefaultMembers()
                .OfType<PropertyInfo>()
                .Where(p => p.GetIndexParameters().Length == idxArgs.Length)
                .ToList();
            return GetMemberValue(src, idxProps, idxArgs).value;
        }

        private static async Task<object> GetIndexed(object src, IReadOnlyList<Expression> idxArguments, IExpressionVisitor expressionVisitor, string filePath)
        {
            var args = await idxArguments.ToAsyncEnumerable().SelectAwait(async x => await expressionVisitor.Visit(x, filePath)).ToArrayAsync().ConfigureAwait(false);
            var t = src.GetType();
            if (t.IsArray)
            {
                var rank = t.GetArrayRank();
                if (rank != args.Length)
                    throw new ArgumentException($"The given array does not have as many dimensions ({rank}) as indices were given ({args.Length}).");
                var arr = (Array)src;
                var idxs = args.Select(x => Convert.ToInt32(x)).ToArray();
                return arr.GetValue(idxs);
            }
            return GetDefaultIndexedPropertyValue(src, args);
        }
    }
}
