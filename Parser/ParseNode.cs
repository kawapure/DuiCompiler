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
    /// <summary>
    /// A base parse node.
    /// </summary>
    internal class ParseNode
#if DEBUG
        : IDebugSerializable
#endif
    {
        /// <summary>
        /// The name of this basic parse node.
        /// </summary>
        /// <remarks>
        /// Child classes should override the Name property.
        /// </remarks>
        private readonly string _name;

        /// <summary>
        /// The name of this node.
        /// </summary>
        public virtual string Name
        {
            get => _name;
            private set { }
        }

        //---------------------------------------------------------------------------------------------------

        /// <summary>
        /// A set of attributes that apply to the node.
        /// </summary>
        public virtual Dictionary<string, string> Attributes { get; protected set; } = new();

        /// <summary>
        /// A list of child nodes of this parse node.
        /// </summary>
        public virtual List<ParseNode> Children { get; protected set; } = new();

        //---------------------------------------------------------------------------------------------------

        /// <summary>
        /// The parent node of this node, or null if there is none.
        /// </summary>
        public virtual ParseNode? ParentNode
        {
            get => m_parentNode;
            protected set { }
        }

        protected ParseNode? m_parentNode = null;

        //---------------------------------------------------------------------------------------------------

        /// <summary>
        /// The origin of the content in the source text which this node
        /// corresponds to.
        /// </summary>
        public virtual SourceOrigin SourceOrigin { get; protected set; }

        //---------------------------------------------------------------------------------------------------

        public ParseNode(SourceOrigin sourceOrigin)
        {
            _name = "ParseNode";
            this.SourceOrigin = sourceOrigin;
        }

        public ParseNode(string name, SourceOrigin sourceOrigin)
            : this(sourceOrigin)
        {
            _name = name;
        }

        //---------------------------------------------------------------------------------------------------

        /// <summary>
        /// Checks the validity of the content (attributes and relationships
        /// to other nodes) of this node.
        /// </summary>
        public virtual bool Validate()
        {
            return true;
        }

        //---------------------------------------------------------------------------------------------------

        /// <summary>
        /// Checks if the node has an attribute set.
        /// </summary>
        public bool HasAttribute(string key)
        {
            return this.Attributes.ContainsKey(key);
        }

        /// <summary>
        /// Sets an attribute to a value.
        /// </summary>
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

        /// <summary>
        /// Gets the value of an attribute if it exists, null otherwise.
        /// </summary>
        public string? GetAttribute(string key)
        {
            if (HasAttribute(key))
            {
                return this.Attributes[key];
            }

            return null;
        }

        //---------------------------------------------------------------------------------------------------

        /// <summary>
        /// Appends another parse node as the child of this one.
        /// </summary>
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

        /// <summary>
        /// Searches the children of this node for a node of a given name.
        /// </summary>
        /// <returns> The queried node if it exists, null otherwise. </returns>
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

        /// <summary>
        /// Sets the parent relationship of this node.
        /// </summary>
        protected void SetParentRelationship(ParseNode parentNode)
        {
            Debug.Assert(parentNode.Children.Contains(this));
            m_parentNode = parentNode;
        }

        /// <summary>
        /// Removes the parent relationship of this node, making it independent.
        /// </summary>
        protected internal void RemoveParentRelationship()
        {
            m_parentNode?.Children.Remove(this);
            m_parentNode = null;
        }

        //---------------------------------------------------------------------------------------------------

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
