﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Irony.Parsing;
using StringExtensions;

namespace ParserTests
{
    [TestFixture]
    class PowerShellGrammarTests
    {
        [Test]
        public void CreateTest()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();
            // obviously I know it won't be null. That's mostly to 
            // avoid the compiler warning.
            Assert.IsNotNull(grammar);
        }

        [Test]
        public void NonTerminalFieldsInitializedTest()
        {
            // created by reflection (to avoid missing one)
            // but let's make sure the reflection works
            var grammar = new PowerShellGrammar.InteractiveInput();
            Assert.IsNotNull(grammar.interactive_input);
            Assert.AreEqual("interactive_input", grammar.interactive_input.Name);
        }

        [TestFixture]
        class ParseSimpleCommandTest
        {
            ParseTree parseTree;
            PowerShellGrammar grammar;

            public ParseSimpleCommandTest()
            {
                grammar = new PowerShellGrammar.InteractiveInput();
                var parser = new Parser(grammar);

                parseTree = parser.Parse("Get-ChildItem");
            }

            [Test]
            public void SuccessfulParseTest()
            {
                Assert.IsNotNull(parseTree);
                Assert.IsFalse(parseTree.HasErrors, parseTree.ParserMessages.JoinString("\n"));
            }

            [Test]
            public void CorrectNonTerminalsTest()
            {
                var expected = new[] { 
                    grammar.interactive_input,
                    grammar.script_block,
                    grammar.script_block_body,
                    grammar.statement_list,
                    grammar.statement,
                    grammar.pipeline,
                    grammar.command,
                    grammar.command_name,
                };

                var node = parseTree.Root;

                foreach (var rule in expected)
                {
                    Assert.AreEqual(rule, node.Term);
                    Assert.AreEqual(1, node.ChildNodes.Count, node.ToString());
                    node = node.ChildNodes.Single();
                }

                Assert.AreEqual(0, node.ChildNodes.Count, node.ToString());
            }
        }

        [Test]
        public void TrivialPromptExpressionsTest()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();

            var parser = new Parser(grammar);
            var parseTree = parser.Parse("\"PS>\"");

            Assert.IsNotNull(parseTree);
            Assert.IsFalse(parseTree.HasErrors, parseTree.ParserMessages.JoinString("\n"));

            var expected = new[] {
                    grammar.interactive_input,
                    grammar.script_block,
                    grammar.script_block_body,
                    grammar.statement_list,
                    grammar.statement,
                    grammar.pipeline,
                    grammar.command,
                    grammar.command_name
                };

            var node = parseTree.Root;

            foreach (var rule in expected)
            {
                Assert.AreEqual(rule, node.Term);
                Assert.AreEqual(1, node.ChildNodes.Count, node.ToString());
                node = node.ChildNodes.Single();
            }

            Assert.AreEqual(0, node.ChildNodes.Count, node.ToString());
        }
    }
}
