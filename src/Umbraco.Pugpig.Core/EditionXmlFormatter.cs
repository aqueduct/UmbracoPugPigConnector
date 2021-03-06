﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Pugpig.Core.Interfaces;
using Umbraco.Pugpig.Core.Models;

namespace Umbraco.Pugpig.Core
{
    public class EditionXmlFormatter : IEditionXmlFormatter
    {
        private readonly IFeedSettings m_feedInfo;

        public EditionXmlFormatter(IFeedSettings feedInfo)
        {
            m_feedInfo = feedInfo;
        }

        public XmlDocument GenerateXml(Feed feed)
        {
            var element = new XElement("feed",
                                       new XElement("id", m_feedInfo.PublicationName),
                                       GetLinkElement(m_feedInfo.PublicationName),
                                       new XElement("title", feed.Title),
                                       new XElement("updated", feed.LastUpdated.ToString("yyyy-MM-ddTH:mm:sszzz")),
                                       GetEntries(feed.Entries));

            XmlReader xmlReader = element.CreateReader();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlReader);

            SetNamespaces(xmlDoc);

            return xmlDoc;
        }

        private XElement GetLinkElement(string publicatinName)
        {
            XElement link = new XElement("link");
            link.SetAttributeValue("rel", "self");
            link.SetAttributeValue("type", "application/atom+xml;profile=opds-catalog;kind=acquisition");
            link.SetAttributeValue("href", String.Concat(m_feedInfo.BaseUrl, "/umbraco/pugpig/pugpigSurface/editions/",publicatinName));
            return link;
        }

        private void SetNamespaces(XmlDocument xmlDoc)
        {
            if (xmlDoc.DocumentElement != null)
            {
                xmlDoc.DocumentElement.SetAttribute("xmlns", "http://www.w3.org/2005/Atom");
                xmlDoc.DocumentElement.SetAttribute("xmlns:dcterms", "http://purl.org/dc/terms/");
                xmlDoc.DocumentElement.SetAttribute("xmlns:opds", "http://opds-spec.org/2010/catalog");
                xmlDoc.DocumentElement.SetAttribute("xmlns:app", "http://www.w3.org/2007/app");
            }
        }

        private List<XElement> GetEntries(List<Entry> entries)
        {
            List<XElement> elements = new List<XElement>();
            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    XNamespace xNamespace = "http://purl.org/dc/terms/";
                    elements.Add(new XElement("entry",
                                              new XElement("title", entry.Title),
                                              new XElement("id", String.Concat("com.umbraco.edition.",entry.Id)),
                                              new XElement("updated", entry.Updated.ToString("yyyy-MM-ddTH:mm:sszzz")),
                                              new XElement("author",
                                              new XElement("name", entry.AuthourName)),

                                              new XAttribute(XNamespace.Xmlns + "dcterms", "http://purl.org/dc/terms/"),
                                              new XElement(xNamespace + "issued", entry.Updated.ToString("yyyy-MM-dd")),


                                            
                                              GetSummary(entry),
                                              GetCoverImage(entry),
                                              GetEditionUrl(entry),
                                              GetAlternateEdition(entry)
                                     ));
                }
            }
            return elements;
        }

        private XElement GetAlternateEdition(Entry entry)
        {
            var editionUrl = new XElement("link");
            editionUrl.SetAttributeValue("rel", "alternate");
            editionUrl.SetAttributeValue("type", "application/atom+xml");
            editionUrl.SetAttributeValue("href", String.Format("/umbraco/pugpig/PugpigSurface/Acquisition?publicationName={0}&edition={1}", m_feedInfo.PublicationName, entry.Title));
            return editionUrl;
        }

        private XElement GetEditionUrl(Entry entry)
        {
            var editionUrl = new XElement("link");
            editionUrl.SetAttributeValue("rel", "http://opds-spec.org/acquisition");
            editionUrl.SetAttributeValue("type", "application/atom+xml");
            editionUrl.SetAttributeValue("href", String.Format("/umbraco/pugpig/PugpigSurface/Acquisition?publicationName={0}&edition={1}", m_feedInfo.PublicationName, entry.Title));

            return editionUrl;
        }

        private XElement GetCoverImage(Entry entry)
        {
            var coverImage = new XElement("link");
            coverImage.SetAttributeValue("rel", "http://opds-spec.org/image");
            coverImage.SetAttributeValue("type", "image/jpg");
            coverImage.SetAttributeValue("href", entry.Image.Url);
            return coverImage;
        }

        private XElement GetSummary(Entry entry)
        {
            var summary = new XElement("summary",entry.Summary);
            summary.SetAttributeValue("type", "text");
            return summary;
        }
    }
}
