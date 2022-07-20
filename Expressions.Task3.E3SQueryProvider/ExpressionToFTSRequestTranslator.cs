using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Expressions.Task3.E3SQueryProvider
{
    public class ExpressionToFtsRequestTranslator : ExpressionVisitor
    {
        private string _member;
        private string _constant;

        public ExpressionToFtsRequestTranslator()
        {
            new StringBuilder();
        }

        public string Translate(Expression exp)
        {
            Visit(exp);

            return $"{_member}:({_constant})";
        }

        #region protected methods

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable)
                && node.Method.Name == "Where")
            {
                var predicate = node.Arguments[1];
                Visit(predicate);

                return node;
            }

            if (node.Method.DeclaringType == typeof(string))
            {
                Visit(node.Arguments[0]);
                Visit(node.Object);
                switch (node.Method.Name)
                {
                    case "Contains":
                        _constant = $"*{_constant}*";
                        break;
                    case "StartsWith":
                        _constant = $"{_constant}*";
                        break;
                    case "EndsWith":
                        _constant = $"*{_constant}";
                        break;
                }
                return node;
            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    Visit(node.Left);
                    Visit(node.Right);
                    break;

                default:
                    throw new NotSupportedException($"Operation '{node.NodeType}' is not supported");
            };

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _member = node.Member.Name;

            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _constant = node.Value.ToString();
            return node;
        }

        #endregion
    }
}
