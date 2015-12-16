using System.Collections.Generic;
using System.IO;
using System.Xaml;
using System.Xml;

namespace WorkflowMicroServicesPoC.Designer
{
    internal class IgnorableXamlXmlWriter : XamlXmlWriter
    {
        HashSet<NamespaceDeclaration> ignorableNamespaces = new HashSet<NamespaceDeclaration>();
        HashSet<NamespaceDeclaration> allNamespaces = new HashSet<NamespaceDeclaration>();
        bool objectWritten;
        bool hasDesignNamespace;
        string designNamespacePrefix;

        public IgnorableXamlXmlWriter(TextWriter tw, XamlSchemaContext context)
            : base(XmlWriter.Create
                       (tw, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }), context, new XamlXmlWriterSettings { AssumeValidInput = true })
        {
        }

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            if (!objectWritten)
            {
                allNamespaces.Add(namespaceDeclaration);

                if (namespaceDeclaration.Namespace == "http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation")
                {
                    hasDesignNamespace = true;
                    designNamespacePrefix = namespaceDeclaration.Prefix;
                }
            }
            base.WriteNamespace(namespaceDeclaration);
        }

        public override void WriteStartObject(XamlType type)
        {
            if (!objectWritten)
            {
                if (hasDesignNamespace)
                {
                    string mcAlias = "mc";
                    this.WriteNamespace(new NamespaceDeclaration("http://schemas.openxmlformats.org/markup-compatibility/2006", mcAlias));
                }
            }
            base.WriteStartObject(type);

            if (!objectWritten)
            {
                if (hasDesignNamespace)
                {
                    XamlDirective ig = new XamlDirective("http://schemas.openxmlformats.org/markup-compatibility/2006", "Ignorable");
                    WriteStartMember(ig);
                    WriteValue(designNamespacePrefix);
                    WriteEndMember();
                    objectWritten = true;
                }
            }
        }
    }
}