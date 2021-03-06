﻿namespace org.apache.lucene.analysis.ga
{

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

	using CharTermAttribute = org.apache.lucene.analysis.tokenattributes.CharTermAttribute;

	/// <summary>
	/// Normalises token text to lower case, handling t-prothesis
	/// and n-eclipsis (i.e., that 'nAthair' should become 'n-athair')
	/// </summary>
	public sealed class IrishLowerCaseFilter : TokenFilter
	{
	  private readonly CharTermAttribute termAtt = addAttribute(typeof(CharTermAttribute));

	  /// <summary>
	  /// Create an IrishLowerCaseFilter that normalises Irish token text.
	  /// </summary>
	  public IrishLowerCaseFilter(TokenStream @in) : base(@in)
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public boolean incrementToken() throws java.io.IOException
	  public override bool incrementToken()
	  {
		if (input.incrementToken())
		{
		  char[] chArray = termAtt.buffer();
		  int chLen = termAtt.length();
		  int idx = 0;

		  if (chLen > 1 && (chArray[0] == 'n' || chArray[0] == 't') && isUpperVowel(chArray[1]))
		  {
			chArray = termAtt.resizeBuffer(chLen + 1);
			for (int i = chLen; i > 1; i--)
			{
			  chArray[i] = chArray[i - 1];
			}
			chArray[1] = '-';
			termAtt.Length = chLen + 1;
			idx = 2;
			chLen = chLen + 1;
		  }

		  for (int i = idx; i < chLen;)
		  {
			i += char.toChars(char.ToLower(chArray[i]), chArray, i);
		  }
		  return true;
		}
		else
		{
		  return false;
		}
	  }

	  private bool isUpperVowel(int v)
	  {
		switch (v)
		{
		  case 'A':
		  case 'E':
		  case 'I':
		  case 'O':
		  case 'U':
		  // vowels with acute accent (fada)
		  case '\u00c1':
		  case '\u00c9':
		  case '\u00cd':
		  case '\u00d3':
		  case '\u00da':
			return true;
		  default:
			return false;
		}
	  }
	}

}