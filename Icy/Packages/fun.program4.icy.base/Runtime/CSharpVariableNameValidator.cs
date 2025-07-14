/*
 * Copyright 2025 @ProgramForFun. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using System.Collections.Generic;

namespace Icy.Base
{
	/// <summary>
	/// 字符串是否是合法的C#变量名的检测器
	/// </summary>
	public static class CSharpVariableValidator
	{
		private static readonly HashSet<string> Keywords = new HashSet<string>
		{
			"abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
			"class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
			"enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
			"foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
			"long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
			"private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
			"sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
			"try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
			"void", "volatile", "while"
		};

		/// <summary>
		/// 指定字符串是否是合法的C#变量名
		/// </summary>
		public static bool IsValidCSharpVariableName(string name)
		{
			if (string.IsNullOrEmpty(name))
				return false;

			bool hasAtPrefix = name.StartsWith("@");
			string identifierPart = hasAtPrefix ? name.Substring(1) : name;

			if (identifierPart.Length == 0)
				return false;

			if (!(char.IsLetter(identifierPart[0]) || identifierPart[0] == '_'))
				return false;

			foreach (char c in identifierPart)
			{
				if (!(char.IsLetterOrDigit(c) || c == '_'))
					return false;
			}

			return hasAtPrefix || !IsKeyword(identifierPart);
		}

		private static bool IsKeyword(string identifier)
		{
			return Keywords.Contains(identifier);
		}
	}
}
