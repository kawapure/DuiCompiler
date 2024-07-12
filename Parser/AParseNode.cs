using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if DEBUG
using Kawapure.DuiCompiler.Debugging;
using System.Xml.Linq;
#endif

namespace Kawapure.DuiCompiler.Parser
{
    internal abstract class AParseNode
#if DEBUG
        : IDebugSerializable
#endif
    {
        public abstract string Name { get; protected set; }

        public virtual Dictionary<string, string> Attributes { get; protected set; } = new();

        public virtual List<AParseNode> Children { get; protected set; } = new();

        public virtual SourceOrigin SourceOrigin { get; protected set; }

        public AParseNode(SourceOrigin sourceOrigin)
        {
            this.SourceOrigin = sourceOrigin;
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

                foreach (AParseNode child in this.Children)
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
