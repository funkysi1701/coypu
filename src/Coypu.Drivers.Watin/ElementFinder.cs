using System;
using System.Text.RegularExpressions;
using WatiN.Core;

namespace Coypu.Drivers.Watin
{
    internal class ElementFinder
    {
        private readonly WatiNDriver driver;
        private Func<IElementContainer> findScope;

        public ElementFinder(WatiNDriver driver)
        {
            this.driver = driver;
            ClearScope();
        }

        private Document GetDocument()
        {
            return driver.Watin;
        }

        internal IElementContainer Scope
        {
            get
            {
                var f = findScope;
                ClearScope();
                var scope = f();
                SetScope(f);
                return scope;
            }
        }

        public void SetScope(Func<IElementContainer> findElementContainer)
        {
            findScope = findElementContainer;
        }

        public void ClearScope()
        {
            findScope = GetDocument;
        }

        public WatiN.Core.Element FindButton(string locator)
        {
            var isButton = Constraints.OfType<Button>()
                           | Find.ByElement(e => e.TagName == "INPUT" && e.GetAttributeValue("type") == "image")
                           | Find.By("role", "button");

            var byText = Find.ByText(locator);
            var byIdNameValueOrAlt = Find.ById(locator)
                                     | Find.ByName(locator)
                                     | Find.ByValue(locator)
                                     | Find.ByAlt(locator);
            var byPartialId = Constraints.WithPartialId(locator);
            var hasLocator = byText | byIdNameValueOrAlt | byPartialId;

            var notHidden = Constraints.NotHidden();

            var candidates = Scope.Elements.Filter(isButton & hasLocator & notHidden);
            return candidates.FirstMatching(byText, byIdNameValueOrAlt, byPartialId);
        }

        public WatiN.Core.Element FindElement(string id)
        {
            return Scope.Elements.First(Find.ById(id) & Constraints.NotHidden());
        }

        public WatiN.Core.Element FindField(string locator)
        {
            var field = FindFieldByLabel(locator);
            if (field == null)
            {
                var isField = Constraints.IsField();

                var byIdOrName = Find.ById(locator) | Find.ByName(locator);
                var byPlaceholder = Find.By("placeholder", locator);
                var radioButtonByValue = Constraints.OfType<RadioButton>() & Find.ByValue(locator);
                var byPartialId = Constraints.WithPartialId(locator);

                var hasLocator = byIdOrName | byPlaceholder | radioButtonByValue | byPartialId;

                var notHidden = Constraints.NotHidden();

                var candidates = Scope.Elements.Filter(isField & hasLocator & notHidden);
                field = candidates.FirstMatching(byIdOrName, byPlaceholder, radioButtonByValue, byPartialId);
            }

            return field;
        }

        private WatiN.Core.Element FindFieldByLabel(string locator)
        {
            WatiN.Core.Element field = null;

            var label = Scope.Labels.First(Find.ByText(new Regex(locator)));
            if (label != null)
            {
                var notHidden = Constraints.NotHidden();

                if (!string.IsNullOrEmpty(label.For))
                    field = Scope.Elements.First(Find.ById(label.For) & notHidden);

                if (field == null)
                    field = label.Elements.First(Constraints.IsField() & notHidden);
            }
            return field;
        }

        public WatiN.Core.Element FindFieldset(string locator)
        {
            var withId = Find.ById(locator);
            var withLegend = Constraints.HasElement("legend", Find.ByText(locator));
            var hasLocator = withId | withLegend;

            var notHidden = Constraints.NotHidden();

            return Scope.Fieldsets().First(hasLocator & notHidden);
        }

        public Frame FindFrame(string locator)
        {
            return GetDocument().Frames.First(Find.ByTitle(locator) | Find.ById(locator) | Constraints.HasElement("h1", Find.ByText(locator)));
        }

        public WatiN.Core.Element FindLink(string linkText)
        {
            return Scope.Links.First(Find.ByText(linkText) & Constraints.NotHidden());
        }

        public WatiN.Core.Element FindSection(string locator)
        {
            var isSection = Constraints.OfType(typeof(Section), typeof(Div));

            var hasLocator = Find.ById(locator)
                             | Constraints.HasElement(new[] { "h1", "h2", "h3", "h4", "h5", "h6" }, Find.ByText(locator));

            var notHidden = Constraints.NotHidden();

            return Scope.Elements.First(isSection & hasLocator & notHidden);
        }
    }
}