using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp_Interpreter.Models
{
    public enum StatementType
    {
        Block,
        If,
        ElseIf,
        Else,
        While,
        For,
        VariableDeclaration,
        VariableAssignement,
        ConsoleOutput,
        ConsoleInput,
        MethodDeclaration,
        Return,
        MethodCall,
    }

    public abstract class Node { } // any node
    public abstract class StatementNode : Node // any statement
    {
        //public abstract StatementType StatementType { get; set; }
    } 

    public abstract class ExpressionNode : Node // any expression
    { 
        public abstract DataType Type { get; set; }
    } 

    public enum AssignmentType
    {
        Equal,
        PlusEqual,
        MinusEqual,
        StarEqual,
        SlashEqual,
        PlusPlus,
        MinusMinus
    }
    public class UsingDirectiveNode : Node 
    {
        public string NameSpace { get; set; }
    }

    public class AssignementNode : StatementNode
    {
        public DataType Type { get; set; }
        public string Name { get; set; }
        public AssignmentType AssignmentType { get; set; }
        public ExpressionNode Expression { get; set; }
    }

    public class DeclarationNode : StatementNode
    {
        public DataType Type { get; set; }
        public string Name { get; set; } = null!;
        public ExpressionNode Expression { get; set; }
    }

    public class IfNode : StatementNode
    {
        public ExpressionNode Condition { get; set; }
        public StatementNode ThenStatements { get; set; }
        public List<ElseIfNode>? ElseIfStatements { get; set; }
        public StatementNode? ElseStatements { get; set; }
    }

    public class ElseIfNode : StatementNode
    {
        public ExpressionNode Condition { get; set; }
        public StatementNode ThenStatements { get; set; }
    }

    public class WhileNode : StatementNode
    {
        public ExpressionNode Condition { get; set; }
        public StatementNode Body { get; set; }
    }

    public class ForExpression
    {
        public DeclarationNode StartPoint { get; set; }
        public ExpressionNode Condition { get; set; }
        public AssignementNode? Steps { get; set; }
    }

    public class ForNode : StatementNode
    {
        public ForExpression ForExpression { get; set; }
        public StatementNode Body { get; set; }
    }
    public class BlockNode : StatementNode
    {
        public List<StatementNode> Statements { get; set; }
    }

    public abstract class ConsoleNode : StatementNode { };

    public class ConsoleOutputNode : ConsoleNode
    {
        public ExpressionNode Object { get; set; }
    }

    public class ConsoleInputNode : ConsoleNode
    {
    }

    public class ParameterNode : Node
    {
        public DataType Type { get; set; }
        public string Name { get; set; }
        public ExpressionNode? DefualtValue { get; set; }
    }

    public class MethodDeclarationNode : StatementNode
    {
        public DataType ReturnType { get; set; }
        public ParameterNode[] Parameters { get; set; }
        public string Name { get; set; }
        public BlockNode Body { get; set; }
    }

    public class ReturnNode : StatementNode
    {
        public DataType ReturnType { get; set; }
        public ExpressionNode Value { get; set; }
    }

    public class MethodCallNode : StatementNode
    {
        public string Name { get; set; }
        public DataType ReturnType { get; set; }
        public object[] Arguments { get; set; }
    }

    public class LiteralNode : ExpressionNode
    {
        public object Value { get; set; }
        public override DataType Type { get; set; }
    }

    public class VariableNode : ExpressionNode
    {
        public string Name { get; set; } = null!;
        public override DataType Type { get; set; }
    }

    public class BinaryExpressionNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }
        public string Operator { get; set; } = null!;
        public ExpressionNode Right { get; set; }
        public override DataType Type { get; set; }
    }

    public class UnaryExpressionNode : ExpressionNode
    {
        public string Operator { get; set; } = null!;
        public ExpressionNode Operand { get; set; }
        public override DataType Type { get; set; }
    }

    public class MethodCallExpressionNode : ExpressionNode
    {
        public string Name { get; set; }
        public object[] Arguments { get; set; }
        public override DataType Type { get; set; }

        public static explicit operator MethodCallExpressionNode(StatementNode v)
        {
            if (v is MethodCallNode methodCall)
            {
                return new MethodCallExpressionNode
                {
                    Type = methodCall.ReturnType,
                    Name = methodCall.Name,
                    Arguments = methodCall.Arguments
                };
            }

            throw new InvalidCastException($"Cannot convert {v.GetType().Name} to MethodCallExpressionNode");
        }
    }

}
