using System;
using System.Collections.Generic;

namespace Lucene.Net.Index
{
    using Lucene.Net.Document;
    using Lucene.Net.Store;

    /*
         * Licensed to the Apache Software Foundation (ASF) under one or more
         * contributor license agreements.  See the NOTICE file distributed with
         * this work for additional information regarding copyright ownership.
         * The ASF licenses this file to You under the Apache License, Version 2.0
         * (the "License"); you may not use this file except in compliance with
         * the License.  You may obtain a copy of the License at
         *
         *     http://www.apache.org/licenses/LICENSE-2.0
         *
         * Unless required by applicable law or agreed to in writing, software
         * distributed under the License is distributed on an "AS IS" BASIS,
         * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
         * See the License for the specific language governing permissions and
         * limitations under the License.
         */

    using Lucene.Net.Util;
    using NUnit.Framework;
    using DocIdSetIterator = Lucene.Net.Search.DocIdSetIterator;

    [TestFixture]
    public class TestStressAdvance : LuceneTestCase
    {
        [Test]
        public virtual void TestStressAdvance_Mem()
        {
            for (int iter = 0; iter < 3; iter++)
            {
                if (VERBOSE)
                {
                    Console.WriteLine("\nTEST: iter=" + iter);
                }
                Directory dir = NewDirectory();
                RandomIndexWriter w = new RandomIndexWriter(Random(), dir);
                HashSet<int> aDocs = new HashSet<int>();
                Document doc = new Document();
                Field f = NewStringField("field", "", Field.Store.NO);
                doc.Add(f);
                Field idField = NewStringField("id", "", Field.Store.YES);
                doc.Add(idField);
                int num = AtLeast(4097);
                if (VERBOSE)
                {
                    Console.WriteLine("\nTEST: numDocs=" + num);
                }
                for (int id = 0; id < num; id++)
                {
                    if (Random().Next(4) == 3)
                    {
                        f.StringValue = "a";
                        aDocs.Add(id);
                    }
                    else
                    {
                        f.StringValue = "b";
                    }
                    idField.StringValue = "" + id;
                    w.AddDocument(doc);
                    if (VERBOSE)
                    {
                        Console.WriteLine("\nTEST: doc upto " + id);
                    }
                }

                w.ForceMerge(1);

                IList<int> aDocIDs = new List<int>();
                IList<int> bDocIDs = new List<int>();

                DirectoryReader r = w.Reader;
                int[] idToDocID = new int[r.MaxDoc()];
                for (int docID = 0; docID < idToDocID.Length; docID++)
                {
                    int id = Convert.ToInt32(r.Document(docID).Get("id"));
                    if (aDocs.Contains(id))
                    {
                        aDocIDs.Add(docID);
                    }
                    else
                    {
                        bDocIDs.Add(docID);
                    }
                }
                TermsEnum te = GetOnlySegmentReader(r).Fields().Terms("field").Iterator(null);

                DocsEnum de = null;
                for (int iter2 = 0; iter2 < 10; iter2++)
                {
                    if (VERBOSE)
                    {
                        Console.WriteLine("\nTEST: iter=" + iter + " iter2=" + iter2);
                    }
                    Assert.AreEqual(TermsEnum.SeekStatus.FOUND, te.SeekCeil(new BytesRef("a")));
                    de = TestUtil.Docs(Random(), te, null, de, DocsEnum.FLAG_NONE);
                    TestOne(de, aDocIDs);

                    Assert.AreEqual(TermsEnum.SeekStatus.FOUND, te.SeekCeil(new BytesRef("b")));
                    de = TestUtil.Docs(Random(), te, null, de, DocsEnum.FLAG_NONE);
                    TestOne(de, bDocIDs);
                }

                w.Dispose();
                r.Dispose();
                dir.Dispose();
            }
        }

        private void TestOne(DocsEnum docs, IList<int> expected)
        {
            if (VERBOSE)
            {
                Console.WriteLine("test");
            }
            int upto = -1;
            while (upto < expected.Count)
            {
                if (VERBOSE)
                {
                    Console.WriteLine("  cycle upto=" + upto + " of " + expected.Count);
                }
                int docID;
                if (Random().Next(4) == 1 || upto == expected.Count - 1)
                {
                    // test nextDoc()
                    if (VERBOSE)
                    {
                        Console.WriteLine("    do nextDoc");
                    }
                    upto++;
                    docID = docs.NextDoc();
                }
                else
                {
                    // test advance()
                    int inc = TestUtil.NextInt(Random(), 1, expected.Count - 1 - upto);
                    if (VERBOSE)
                    {
                        Console.WriteLine("    do advance inc=" + inc);
                    }
                    upto += inc;
                    docID = docs.Advance(expected[upto]);
                }
                if (upto == expected.Count)
                {
                    if (VERBOSE)
                    {
                        Console.WriteLine("  expect docID=" + DocIdSetIterator.NO_MORE_DOCS + " actual=" + docID);
                    }
                    Assert.AreEqual(DocIdSetIterator.NO_MORE_DOCS, docID);
                }
                else
                {
                    if (VERBOSE)
                    {
                        Console.WriteLine("  expect docID=" + expected[upto] + " actual=" + docID);
                    }
                    Assert.IsTrue(docID != DocIdSetIterator.NO_MORE_DOCS);
                    Assert.AreEqual((int)expected[upto], docID);
                }
            }
        }
    }
}