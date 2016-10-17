using System;
using CSharpGuidelinesAnalyzer.ClassDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.ClassDesign
{
    public class MembersShouldDoASingleThingSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => MembersShouldDoASingleThingAnalyzer.DiagnosticId;

        [Fact]
        public void When_property_name_contains_the_word_And_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        object [|CustomerAndOrder|] { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Property 'CustomerAndOrder' contains the word 'and'.");
        }

        [Fact]
        public void When_method_name_contains_the_word_And_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    struct C
                    {
                        void [|SaveCustomerAndOrder|]()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'SaveCustomerAndOrder' contains the word 'and'.");
        }

        [Fact]
        public void When_field_name_contains_the_word_And_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        object [|_customerAndOrder|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Field '_customerAndOrder' contains the word 'and'.");
        }

        [Fact]
        public void When_event_name_contains_the_word_And_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public event EventHandler [|CustomerAndOrderSaved|]
                        {
                            add 
                            { 
                                throw new NotImplementedException(); 
                            }
                            remove 
                            { 
                                throw new NotImplementedException(); 
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Event 'CustomerAndOrderSaved' contains the word 'and'.");
        }

        [Fact]
        public void When_method_name_contains_the_word_and_with_underscores_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|save_customer_and_order|]()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'save_customer_and_order' contains the word 'and'.");
        }

        [Fact]
        public void When_method_name_contains_the_word_and_with_digits_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|Match1And2|]()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'Match1And2' contains the word 'and'.");
        }

        [Fact]
        public void When_method_name_contains_the_word_Land_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void ExploreTheBigLand()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_method_name_contains_the_word_Andy_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void AndyWithJohn()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new MembersShouldDoASingleThingAnalyzer();
        }

        protected override CodeFixProvider CreateFixProvider()
        {
            throw new NotImplementedException();
        }
    }
}