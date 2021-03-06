/*
 *   PolishNotation.cs: implements reverse Polish notation for reading and handling simple math expressions.
 *   Copyright (C) 2014-2015  Pavel Vasilev
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

/*
 * Please do not use Derivative() more than once. In fact, avoid using it unless really needed,
 * as it is somewhat slow and stupid (yet functioning).
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace PolishNotation
{
	public class PolishNotation
	{
		private static char[] operators = { '+', '-', '*', '/', '^', '(', ')' };
		/* functions in which identical starting substrings exist should be put in order of increasing length in `functions` and reversely in `functions_reverse` */
		private static string[] functions = new string[] { "sin", "cos", "exp", "abs", "log", "sgn", "erf", "sqrt", "sinh", "cosh", "arcsin", "arccos" };
		private static string[] functions_reverse = new string[] { "arccos", "arcsin", "cosh", "sinh", "sqrt", "erf", "sgn", "log", "abs", "exp", "cos", "sin" };

		private string contents = "";
		private uint calcs = 0;

		public uint CalculationsMade
		{
			get
			{
				return calcs;
			}
		}

		/// <summary>
		/// Converts plaintext to Polish notation
		/// </summary>
		/// <param name="input">Input string in form of simple math expression, e.g. `sin(e^-x)`</param>
		/// <returns></returns>
		public static PolishNotation FromEquation(string input)
		{
			input = input.Trim().Replace(" ", "");
			StringBuilder sb = new StringBuilder();
			StringBuilder buf = new StringBuilder();
			string function_buffer = "";
			Stack<string> stack = new Stack<string>();

			for (int i = 0; i < input.Length; i++)
			{
				if (operators.Contains(input[i]))
				{
					//string last = stack.Peek();

					if (buf.Length > 0)
					{
						sb.Append(buf + " ");
						buf.Clear();
					}
					if (input[i] == '+' || input[i] == '-')
					{
						while (stack.Count > 0 && (stack.Peek() == "+" || stack.Peek() == "-" || stack.Peek() == "*" || stack.Peek() == "/"
							|| stack.Peek() == "^" || functions.Contains(stack.Peek())))
							sb.Append(stack.Pop() + " ");
					}
					else if (input[i] == '*' || input[i] == '/')
					{
						while (stack.Count > 0 && (stack.Peek() == "*" || stack.Peek() == "/" || stack.Peek() == "^" ||
							functions.Contains(stack.Peek())))
							sb.Append(stack.Pop() + " ");
					}
					else if (input[i] == '^')
					{
						while (stack.Count > 0 && functions.Contains(stack.Peek()))
							sb.Append(stack.Pop() + " ");
					}

					if (input[i] == ')')
					{
						while (!stack.Peek().Equals("("))
						{
							sb.Append(stack.Pop() + " ");
						}

						if (functions.Contains(stack.Peek()))
							sb.Append(stack.Pop() + " ");
						else
							stack.Pop();
					}
					else
						stack.Push(input[i].ToString());

				}
				// TODO: optimize this
				else if (functions_reverse.Any(o => // some cruel lambda stuff
				{
					try
					{
						if (o == input.Substring(i, o.Length))
						{
							function_buffer = o;
							return true;
						}
						else
							return false;
					}
					catch
					{
						return false;
					}
				}
					) && i < input.Length - function_buffer.Length)
				{
					stack.Push(function_buffer);
					i += function_buffer.Length - 1;
				}

				else
				{
					buf.Append(input[i]);
				}

				if (i == input.Length - 1)
				{
					if (buf.Length > 0)
					{
						sb.Append(buf + " ");
						buf.Clear();
					}

					while (stack.Count > 0)
						sb.Append(stack.Pop() + " ");

				}
			}
			return new PolishNotation(sb.ToString());
		}

		public PolishNotation(string input)
		{
			contents = input;
		}

		public bool Validate()
		{
			try
			{
				Eval(0);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public double Eval(double x = 0, double y = 0, double z = 0)
		{
			calcs++;
			Stack<double> stack = new Stack<double>();
			string[] symbols = contents.TrimEnd().Split(' ');
			for (int i = 0; i < symbols.Count(); i++)
			{
				switch (symbols[i])
				{
					// OPERATORS
					case "+":
						stack.Push(stack.Pop() + stack.Pop());
						break;

					case "-":
						if (stack.Count == 1)
							stack.Push(-stack.Pop());
						else
							stack.Push(-stack.Pop() + stack.Pop());
						break;

					case "*":
						stack.Push(stack.Pop() * stack.Pop());
						break;

					case "/":
						{
							double tmp = stack.Pop();
							stack.Push(stack.Pop() / tmp);
							break;
						}

					case "^":
						{
							double tmp = stack.Pop();
							stack.Push(Math.Pow(stack.Pop(), tmp));
							break;
						}

					// SYMBOLICS
					case "x":
						stack.Push(x);
						break;
					case "y":
						stack.Push(y);
						break;
					case "z":
						stack.Push(z);
						break;
					case "e":
						stack.Push(Math.E);
						break;
					case "pi":
						stack.Push(Math.PI);
						break;
					case "":
						break;

					// FUNCTIONS
					case "arcsin":
						stack.Push(Math.Asin(stack.Pop()));
						break;
					case "arccos":
						stack.Push(Math.Acos(stack.Pop()));
						break;
					case "sin":
						stack.Push(Math.Sin(stack.Pop()));
						break;
					case "cos":
						stack.Push(Math.Cos(stack.Pop()));
						break;
					case "exp":
						stack.Push(Math.Pow(Math.E, stack.Pop()));
						break;
					case "sqrt":
						stack.Push(Math.Sqrt(stack.Pop()));
						break;
					case "sinh":
						stack.Push(Math.Sinh(stack.Pop()));
						break;
					case "cosh":
						stack.Push(Math.Cosh(stack.Pop()));
						break;
					case "abs":
						stack.Push(Math.Abs(stack.Pop()));
						break;
					case "log":
						stack.Push(Math.Log(stack.Pop()));
						break;
					case "sgn":
						stack.Push(Math.Sign(stack.Pop()));
						break;
					case "erf":
						stack.Push(Erf(stack.Pop()));
						break;

					// NUMBERS
					default:
						{
							double tmp = 0;
							double.TryParse(symbols[i], out tmp);
							stack.Push(tmp);
							break;
						}
				}

			}
			if (stack.Count > 0)
				return stack.Pop();
			else
				return 0;

		}

		private double Erfc(double x)
		{
			return 1.0 - Erf(x);
		}

		private double Erf(double x)
		{
			double t = 1.0 / (1.0 + 0.5 * Math.Abs(x));
			double tau = t * Math.Exp(-x * x - 1.26551223 + t * (1.00002368 + t * (0.37409196 + t * (0.09678418 + t * (-0.18628806 +
				t * (0.27886807 + t * (-1.13520398 + t * (1.48851587 + t * (-0.82215223 + t * (0.17087277))))))))));

			return Math.Sign(x) * (1.0 - tau);
		}

		/// <summary>
		/// Computes an analytic derivative from Polish notation in form of Polish notation.
		/// </summary>
		/// <param name="input">Input function</param>
		/// <returns>Derivative of input</returns>
		public static PolishNotation Derivative(PolishNotation input)
		{
			return Derivative(input.ToString(), true);
		}

		private static PolishNotation Derivative(string input, bool der = true)
		{
			if ((input = input.Trim()) == "")
				return new PolishNotation("");

			Stack<string> stack = new Stack<string>();
			string[] symbols;

			symbols = input.Split(' ').ToArray();


			string r = "", l = "", r_orig = "", l_orig = "";
			int r_olength = 0;

			r = Derivative(string.Join(" ", symbols.Take(symbols.Length - 1)), der).ToString();
			r_orig = Derivative(string.Join(" ", symbols.Take(symbols.Length - 1)), false).ToString();
			r_olength = r_orig.Trim().Split(' ').Count();

			switch (symbols[symbols.Length - 1])
			{
				// SYMBOLICS
				case "x":
					if (!der)
						return new PolishNotation("x ");
					return new PolishNotation("1 ");

				case "":
					return new PolishNotation("");

				// OPERATORS
				case "+":
					{
						l = Derivative(string.Join(" ", symbols.Take(symbols.Length - r_olength - 1)), der).ToString();
						if (l == "0 " && r == "0 ")
							return new PolishNotation("0 ");
						if (l == "0 ")
							return new PolishNotation(r);
						if (r == "0 ")
							return new PolishNotation(l);
						double tmp_l, tmp_r;
						if (double.TryParse(l, out tmp_l) && double.TryParse(r, out tmp_r))
							return new PolishNotation(tmp_l + tmp_r + " ");
						return new PolishNotation(l + r + "+ ");
					}
				case "-":
					l = Derivative(string.Join(" ", symbols.Take(symbols.Length - r_olength - 1)), der).ToString();
					if (l == "0 " && r == "0 ")
						return new PolishNotation("0 ");
					if (r == "0 ")
						return new PolishNotation(l);
					return new PolishNotation(l + r + "- ");
				case "*":
					{
						l_orig = Derivative(string.Join(" ", symbols.Take(symbols.Length - r_olength - 1)), false).ToString();

						if (!der) // if no derivation is required, return is unchanged
							return new PolishNotation(l_orig + r + "* ");

						l = Derivative(string.Join(" ", symbols.Take(symbols.Length - r_olength - 1)), der).ToString();

						#region optimizations
						// prepare yourself
						if (l == "1 ")
							if (r_orig == "1 ")
								if (r == "1 ")
									if (l_orig == "1 ")
										return new PolishNotation("2 ");
									else
										return new PolishNotation("1 " + l_orig + "+ ");
								else
									if (l_orig == "1 ")
									return new PolishNotation("1 " + r + "+ ");
								else
									return new PolishNotation("1 " + r + l_orig + "* + ");
							else
								if (r == "1 ")
								if (l_orig == "1 ")
									return new PolishNotation(r_orig + "1 + ");
								else
									return new PolishNotation(r_orig + l_orig + "+ ");
							else
									if (l_orig == "1 ")
								return new PolishNotation(r_orig + r + "+ ");
							else
								return new PolishNotation(r_orig + r + l_orig + "* + ");
						else
							if (r_orig == "1 ")
							if (r == "1 ")
								if (l_orig == "1 ")
									return new PolishNotation(l + "1 + ");
								else
									return new PolishNotation(l + l_orig + "+ ");
							else
								if (l_orig == "1 ")
								return new PolishNotation(l + r + "+ ");
							else
								return new PolishNotation(l + r + l_orig + "* + ");
						else
								if (r == "1 ")
							if (l_orig == "1 ")
								return new PolishNotation(l + r_orig + "* " + "1 + ");
							else
								return new PolishNotation(l + r_orig + "* " + l_orig + "+ ");
						else
									if (l_orig == "1 ")
							return new PolishNotation(l + r_orig + "* " + r + "+ ");

						if (l != "0 " && r_orig != "0 ")
							if (r != "0 " && l_orig != "0 ")
								return new PolishNotation(l + r_orig + "* " + r + l_orig + "* + ");
							else
								return new PolishNotation(l + r_orig + "* ");
						else if (r != "0 " && l_orig != "0 ")
							return new PolishNotation(r + l_orig + "* ");
						else
							return new PolishNotation("0 ");
						#endregion
					}
				case "/":
					{
						l_orig = Derivative(string.Join(" ", symbols.Take(symbols.Length - r_olength - 1)), false).ToString();
						l = Derivative(string.Join(" ", symbols.Take(symbols.Length - r_olength - 1)), der).ToString();
						return new PolishNotation(l + r_orig + "* " + r + l_orig + "* - " + r_orig + "2 ^ / "); // more optimizations coming
					}
				case "^":
					{
						l_orig = Derivative(string.Join(" ", symbols.Take(symbols.Length - r_olength - 1)), false).ToString();

						if (!der)
							return new PolishNotation(l_orig + r + "^ ");

						double tmp = 0;
						l = Derivative(string.Join(" ", symbols.Take(symbols.Length - r_olength - 1)), der).ToString();

						if (double.TryParse(r_orig, out tmp) || IsNumeric(r_orig))
							if (l == "1 ")
							{
								if (tmp == 2)
									return new PolishNotation(r_orig + l_orig + "* ");
								else
									return new PolishNotation(r_orig + l_orig + (tmp - 1) + " ^ * ");
							}
							else
							{
								if (tmp == 2)
									return new PolishNotation(l + r_orig + l_orig + "* * ");
								else
									return new PolishNotation(l + r_orig + l_orig + (tmp - 1) + " ^ * * ");
							}

						if (l_orig == "1 ")
							return new PolishNotation(r_orig + l + "* ");

						return new PolishNotation(l_orig + r_orig + "1 - ^ " + r_orig + l + "* " + l_orig + l_orig + "log * " + r + "* + * ");
					}

				// FUNCTIONS
				case "sin":
					if (!der)
						return new PolishNotation(r + "sin ");
					return new PolishNotation(r_orig + "cos " + r + "* ");
				case "cos":
					if (!der)
						return new PolishNotation(r + "cos ");
					return new PolishNotation("0 " + r_orig + "sin - " + r + "* ");
				case "exp":
					if (!der)
						return new PolishNotation(r_orig + "exp ");
					return new PolishNotation(r_orig + "exp " + r + "* ");
				case "sqrt":
					if (!der)
						return new PolishNotation(r_orig + "sqrt ");
					return new PolishNotation(r + r_orig + "sqrt 2 * / ");
				case "sinh":
					if (!der)
						return new PolishNotation(r + "sinh ");
					return new PolishNotation(r_orig + "cosh " + r + "* ");
				case "cosh":
					if (!der)
						return new PolishNotation(r + "cosh ");
					return new PolishNotation(r_orig + "sinh " + r + "* ");
				case "abs":
					if (!der)
						return new PolishNotation(r + "abs ");
					return new PolishNotation(r + r_orig + "sgn * ");
				case "sgn":
					if (!der)
						return new PolishNotation(r + "sgn ");
					return new PolishNotation("0 ");
				case "log":
					if (!der)
						return new PolishNotation(r + "log ");
					return new PolishNotation(r + r_orig + "/ ");
				case "arcsin":
					if (!der)
						return new PolishNotation(r + "arcsin ");
					return new PolishNotation(r + "1 " + r_orig + "2 ^ - sqrt / ");
				case "arccos":
					if (!der)
						return new PolishNotation(r + "arcsin ");
					return new PolishNotation("0 " + r + "1 " + r_orig + "2 ^ - sqrt / - ");

				// NUMBERS
				default:
					if (!der)
						return new PolishNotation(symbols[symbols.Length - 1] + " ");
					return new PolishNotation("0 ");
			}
		}

		private static bool IsNumeric(string input)
		{
			string[] arr = input.Trim().Split(' ');
			foreach (string s in arr)
				if (s.Equals("x"))
					return false;
			return true;
		}

		public override string ToString()
		{
			return contents;
		}

	}
}
