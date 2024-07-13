using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#if DEBUG
using Kawapure.DuiCompiler.Debugging;
using System.Xml.Linq;
#endif

namespace Kawapure.DuiCompiler.Parser
{
    internal class ParseNode
#if DEBUG
        : IDebugSerializable
#endif
    {
        private readonly string m_name;

        public virtual string Name
        {
            get => m_name;
            private set { }
        }

        public virtual Dictionary<string, string> Attributes { get; protected set; } = new();

        public virtual List<ParseNode> Children { get; protected set; } = new();

        public virtual ParseNode? ParentNode
        {
            get => m_parentNode;
            protected set { }
        }

        public virtual SourceOrigin SourceOrigin { get; protected set; }

        protected ParseNode? m_parentNode = null;

        public ParseNode(SourceOrigin sourceOrigin)
        {
            m_name = "ParseNode";
            this.SourceOrigin = sourceOrigin;
        }

        public ParseNode(string name, SourceOrigin sourceOrigin)
            : this(sourceOrigin)
        {
            m_name = name;
        }

        public virtual bool Validate()
        {
            return true;
        }

        public bool HasAttribute(string key)
        {
            return this.Attributes.ContainsKey(key);
        }

        public void SetAttribute(string key, string value)
        {
            if (HasAttribute(key))
            {
                this.Attributes[key] = value;
            }
            else
            {
                this.Attributes.Add(key, value);
            }
        }

        public string? GetAttribute(string key)
        {
            if (HasAttribute(key))
            {
                return this.Attributes[key];
            }

            return null;
        }

        public void AppendChild(ParseNode childNode)
        {
            // If, for whatever reason, this node is already the child of
            // another node, then we want to remove it from that position.
            if (childNode.ParentNode != null)
            {
                childNode.RemoveParentRelationship();
            }

            this.Children.Add(childNode);
            childNode.SetParentRelationship(this);
        }

        public ParseNode? GetChildByName(string name)
        {
            foreach (ParseNode node in this.Children)
            {
                if (node.Name == name)
                {
                    return node;
                }
            }

            return null;
        }

        protected void SetParentRelationship(ParseNode parentNode)
        {
            Debug.Assert(parentNode.Children.Contains(this));
            m_parentNode = parentNode;
        }

        protected internal void RemoveParentRelationship()
        {
            m_parentNode?.Children.Remove(this);
            m_parentNode = null;
        }

#if DEBUG
        public virtual XElement DebugSerialize()
        {
            XElement result = new("ParseNode");
            result.SetAttributeValue("NativeClassName", this.GetType().Name);
            result.SetAttributeValue("Name", this.Name.ToString());

            if (this.SourceOrigin.sourceProvider is not SourceFile)
            {
                result.SetAttributeValue("AnonymousSource", "true");
            }
            else
            {
                SourceFile sourceFile = (SourceFile)this.SourceOrigin.sourceProvider;

                result.SetAttributeValue(
                    "SourceFile",
                    sourceFile.Path
                );
            }

            if (this.Attributes.Count > 0)
            {
                XElement attributesNode = new("Attributes");

                foreach (KeyValuePair<string, string> kvp in this.Attributes)
                {
                    XElement attributeNode = new("ParseNodeAttribute");
                    attributeNode.SetAttributeValue(kvp.Key, kvp.Value);
                    attributesNode.Add(attributeNode);
                }

                result.Add(attributesNode);
            }

            if (this.Children.Count > 0)
            {
                XElement childrenNode = new("Children");

                foreach (ParseNode child in this.Children)
                {
                    XElement childNode = child.DebugSerialize();
                    childrenNode.Add(childNode);
                }

                result.Add(childrenNode);
            }

            return result;
        }
#endif
    }
}
