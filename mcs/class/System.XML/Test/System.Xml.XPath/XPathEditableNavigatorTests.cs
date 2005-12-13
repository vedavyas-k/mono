//
// XPathEditableNavigatorTests.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
//

#if NET_2_0

using System;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;

namespace MonoTests.System.Xml.XPath
{
	[TestFixture]
	public class XPathEditableNavigatorTests
	{
		private XPathNavigator GetInstance (string xml)
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			return doc.CreateNavigator ();
		}

		private static void AssertNavigator (string label, XPathNavigator nav, XPathNodeType type, string prefix, string localName, string ns, string name, string value, bool hasAttributes, bool hasChildren, bool isEmptyElement)
		{
			label += nav.GetType ();
			Assert.AreEqual (type, nav.NodeType, label + "NodeType");
			Assert.AreEqual (prefix, nav.Prefix, label + "Prefix");
			Assert.AreEqual (localName, nav.LocalName, label + "LocalName");
			Assert.AreEqual (ns, nav.NamespaceURI, label + "Namespace");
			Assert.AreEqual (name, nav.Name, label + "Name");
			Assert.AreEqual (value, nav.Value, label + "Value");
			Assert.AreEqual (hasAttributes, nav.HasAttributes, label + "HasAttributes");
			Assert.AreEqual (hasChildren, nav.HasChildren, label + "HasChildren");
			Assert.AreEqual (isEmptyElement, nav.IsEmptyElement, label + "IsEmptyElement");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void AppendChildStartDocumentInvalid ()
		{
			XPathNavigator nav = GetInstance (String.Empty);
			XmlWriter w = nav.AppendChild ();
			w.WriteStartDocument ();
			w.Close ();
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void AppendChildStartAttributeInvalid ()
		{
			XPathNavigator nav = GetInstance (String.Empty);
			XmlWriter w = nav.AppendChild ();
			// Seems like it is just ignored.
			w.WriteStartAttribute ("test");
			w.WriteEndAttribute ();
			w.Close ();
			Assert.AreEqual (XPathNodeType.Root, nav.NodeType, "#1");
			Assert.IsFalse (nav.MoveToFirstChild (), "#2");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void AppendChildElementIncomplete ()
		{
			XPathNavigator nav = GetInstance (String.Empty);
			XmlWriter w = nav.AppendChild ();
			w.WriteStartElement ("foo");
			w.Close ();
		}

		[Test]
		public void AppendChildElement ()
		{
			XPathNavigator nav = GetInstance ("<root/>");
			nav.MoveToFirstChild ();
			XmlWriter w = nav.AppendChild ();
			w.WriteStartElement ("foo");
			w.WriteEndElement ();
			w.Close ();
			Assert.IsTrue (nav.MoveToFirstChild ());
			AssertNavigator ("#1", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"foo",		// LocalName
				String.Empty,	// NamespaceURI
				"foo",		// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				true);		// IsEmptyElement
		}

		[Test]
		public void AppendChildStringFragment ()
		{
			// check that the input string inherits
			// namespace context.
			XPathNavigator nav = GetInstance ("<root xmlns='urn:foo'/>");
			nav.MoveToFirstChild ();
			nav.AppendChild ("<child/>fragment<child></child>");

			Assert.IsTrue (nav.MoveToFirstChild (), "#1-1");
			AssertNavigator ("#1-2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"child",	// LocalName
				"urn:foo",	// NamespaceURI
				"child",	// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				true);		// IsEmptyElement

			Assert.IsFalse (nav.MoveToFirstChild (), "#2-1");
			Assert.IsTrue (nav.MoveToNext (), "#2-2");
			AssertNavigator ("#2-3", nav,
				XPathNodeType.Text,
				String.Empty,	// Prefix
				String.Empty,	// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Name
				"fragment",	// Value
				false,		// HasAttributes
				false,		// HasChildren
				false);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToNext (), "#3-1");
			AssertNavigator ("#3-2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"child",	// LocalName
				"urn:foo",	// NamespaceURI
				"child",	// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				false);		// IsEmptyElement
		}

		[Test]
		public void AppendChildStringInvalidFragment ()
		{
			XPathNavigator nav = GetInstance ("<root xmlns='urn:foo'/>");
			nav.MoveToFirstChild ();
			nav.AppendChild ("<?xml version='1.0'?><root/>");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AppendChildToTextNode ()
		{
			XPathNavigator nav = GetInstance ("<root>text</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild ();
			XmlWriter w = nav.AppendChild ();
		}

		[Test]
		public void InsertAfter ()
		{
			XPathNavigator nav = GetInstance ("<root>test</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild ();
			nav.InsertAfter ("<blah/><doh>sample</doh>");

			AssertNavigator ("#1", nav,
				XPathNodeType.Text,
				String.Empty,	// Prefix
				String.Empty,	// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Name
				"test",		// Value
				false,		// HasAttributes
				false,		// HasChildren
				false);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToNext (), "#2");
			AssertNavigator ("#2-2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"blah",		// LocalName
				String.Empty,	// NamespaceURI
				"blah",		// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				true);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToNext (), "#3");
			AssertNavigator ("#3-2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"doh",		// LocalName
				String.Empty,	// NamespaceURI
				"doh",		// Name
				"sample",	// Value
				false,		// HasAttributes
				true,		// HasChildren
				false);		// IsEmptyElement
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InsertAfterRoot ()
		{
			XPathNavigator nav = GetInstance ("<root/>");
			nav.InsertAfter ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InsertAfterAttribute ()
		{
			XPathNavigator nav = GetInstance ("<root a='b'/>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstAttribute ();
			nav.InsertAfter ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InsertAfterNamespace ()
		{
			XPathNavigator nav = GetInstance ("<root xmlns='urn:foo'/>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstNamespace ();
			nav.InsertAfter ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		// xmlns:xml='...', which is likely to have XmlElement or XmlDocument as its node.
		public void InsertAfterNamespace2 ()
		{
			XPathNavigator nav = GetInstance ("<root />");
			nav.MoveToFirstChild ();
			nav.MoveToFirstNamespace ();
			nav.InsertAfter ();
		}

		[Test]
		public void InsertBefore ()
		{
			XPathNavigator nav = GetInstance ("<root>test</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild ();
			nav.InsertBefore ("<blah/><doh>sample</doh>");

			AssertNavigator ("#1", nav,
				XPathNodeType.Text,
				String.Empty,	// Prefix
				String.Empty,	// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Name
				"test",		// Value
				false,		// HasAttributes
				false,		// HasChildren
				false);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToFirst (), "#2-1");
			AssertNavigator ("#2-2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"blah",		// LocalName
				String.Empty,	// NamespaceURI
				"blah",		// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				true);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToNext (), "#3");
			AssertNavigator ("#3-2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"doh",		// LocalName
				String.Empty,	// NamespaceURI
				"doh",		// Name
				"sample",	// Value
				false,		// HasAttributes
				true,		// HasChildren
				false);		// IsEmptyElement
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InsertBeforeRoot ()
		{
			XPathNavigator nav = GetInstance ("<root/>");
			nav.InsertBefore ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InsertBeforeAttribute ()
		{
			XPathNavigator nav = GetInstance ("<root a='b'/>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstAttribute ();
			nav.InsertBefore ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InsertBeforeNamespace ()
		{
			XPathNavigator nav = GetInstance ("<root xmlns='urn:foo'/>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstNamespace ();
			nav.InsertBefore ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		// xmlns:xml='...', which is likely to have XmlElement or XmlDocument as its node.
		public void InsertBeforeNamespace2 ()
		{
			XPathNavigator nav = GetInstance ("<root />");
			nav.MoveToFirstChild ();
			nav.MoveToFirstNamespace ();
			nav.InsertBefore ();
		}

		[Test]
		public void DeleteRange ()
		{
			XPathNavigator nav = GetInstance ("<root><foo><bar/><baz/></foo><next>child<tmp/></next>final</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // <foo>
			XPathNavigator end = nav.Clone ();
			end.MoveToNext (); // <next>
			end.MoveToNext (); // final
			nav.DeleteRange (end);

			AssertNavigator ("#1", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"root",		// LocalName
				String.Empty,	// NamespaceURI
				"root",		// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				false);		// IsEmptyElement
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DeleteRangeNullArg ()
		{
			XPathNavigator nav = GetInstance ("<root><foo><bar/><baz/></foo><next>child<tmp/></next>final</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // <foo>
			nav.DeleteRange (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DeleteRangeInvalidArg ()
		{
			XPathNavigator nav = GetInstance ("<root><foo><bar/><baz/></foo><next>child<tmp/></next>final</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // <foo>

			XPathNavigator end = nav.Clone ();
			end.MoveToNext (); // <next>
			end.MoveToFirstChild (); // child
			nav.DeleteRange (end);
		}

		[Test]
		public void ReplaceRange ()
		{
			XPathNavigator nav = GetInstance ("<root><foo><bar/><baz/></foo><next>child<tmp/></next>final</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // <foo>

			XPathNavigator end = nav.Clone ();
			end.MoveToNext (); // <next>
			XmlWriter w = nav.ReplaceRange (end);

			AssertNavigator ("#1", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"foo",		// LocalName
				String.Empty,	// NamespaceURI
				"foo",		// Name
				String.Empty,	// Value
				false,		// HasAttributes
				true,		// HasChildren
				false);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToParent (), "#1-2");

			w.WriteStartElement ("whoa");
			w.WriteEndElement ();
			w.Close ();

			AssertNavigator ("#2", nav,
				XPathNodeType.Element,
				String.Empty,	// Prefix
				"whoa",		// LocalName
				String.Empty,	// NamespaceURI
				"whoa",		// Name
				String.Empty,	// Value
				false,		// HasAttributes
				false,		// HasChildren
				true);		// IsEmptyElement

			Assert.IsTrue (nav.MoveToNext (), "#2-1");

			AssertNavigator ("#3", nav,
				XPathNodeType.Text,
				String.Empty,	// Prefix
				String.Empty,	// LocalName
				String.Empty,	// NamespaceURI
				String.Empty,	// Name
				"final",	// Value
				false,		// HasAttributes
				false,		// HasChildren
				false);		// IsEmptyElement
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ReplaceRangeNullArg ()
		{
			XPathNavigator nav = GetInstance ("<root><foo><bar/><baz/></foo><next>child<tmp/></next>final</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // <foo>
			nav.ReplaceRange (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReplaceRangeInvalidArg ()
		{
			XPathNavigator nav = GetInstance ("<root><foo><bar/><baz/></foo><next>child<tmp/></next>final</root>");
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild (); // <foo>

			XPathNavigator end = nav.Clone ();
			end.MoveToNext (); // <next>
			end.MoveToFirstChild (); // child
			nav.ReplaceRange (end);
		}
	}
}

#endif
