using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Coypu.Robustness;

namespace Coypu
{
    internal class DriverScope : Scope<DriverScope>
    {
        private readonly ElementFinder elementFinder;
        protected Driver driver;
        protected RobustWrapper robustWrapper;
        internal Clicker clicker;
        internal UrlBuilder urlBuilder;
        internal StateFinder stateFinder;
        internal TemporaryTimeouts temporaryTimeouts;
        private bool consideringInvisibleElements;

        internal DriverScope(Driver driver, RobustWrapper robustWrapper, Waiter waiter, UrlBuilder urlBuilder)
        {
            this.driver = driver;
            this.robustWrapper = robustWrapper;
            clicker = new Clicker(driver, waiter);
            this.urlBuilder = urlBuilder;
            temporaryTimeouts = new TemporaryTimeouts();
            stateFinder = new StateFinder(robustWrapper, temporaryTimeouts);
        }

        internal DriverScope(ElementFinder elementFinder, DriverScope outer)
        {
            this.elementFinder = elementFinder;
            driver = outer.driver;
            robustWrapper = outer.robustWrapper;
            clicker = outer.clicker;
            urlBuilder = outer.urlBuilder;
            temporaryTimeouts = outer.temporaryTimeouts;
            stateFinder = outer.stateFinder;
        }

        /// <summary>
        /// Click a button, input of type button|submit|image or div with the css class "button"
        /// </summary>
        /// <param name="locator">The text/value, name or id of the button</param>
        /// <exception cref="T:Coypu.MissingHtmlException">Thrown if the element cannot be found</exception>
        public DriverScope ClickButton(string locator)
        {
            RetryUntilTimeout(() => clicker.FindAndClickButton(locator));
            return this;
        }

        /// <summary>
        /// Click the first matching link
        /// </summary>
        /// <param name="locator">The text of the link</param>
        /// <exception cref="T:Coypu.MissingHtmlException">Thrown if the element cannot be found</exception>
        public DriverScope ClickLink(string locator)
        {
            RetryUntilTimeout(() => clicker.FindAndClickLink(locator));
            return this;
        }

        /// <summary>
        /// Click a previously found element
        /// </summary>
        /// <param name="element">The element to click</param>
        /// <exception cref="T:Coypu.MissingHtmlException">Thrown if the element cannot be found</exception>
        public DriverScope Click(Element element)
        {
            RetryUntilTimeout(() => driver.Click(element));
            return this;
        }

        /// <summary>
        /// Find and click an element robustly
        /// </summary>
        /// <param name="findElement">How to find the element</param>
        /// <exception cref="T:Coypu.MissingHtmlException">Thrown if the element cannot be found</exception>
        public DriverScope Click(Func<Element> findElement)
        {
            RetryUntilTimeout(() => driver.Click(findElement()));
            return this;
        }

        /// <summary>
        /// <para>Click a button, input of type button|submit|image or div with the css class "button".</para>
        /// <para>Wait for a condition to be satisfied for a specified time otherwise click and wait again.</para>
        /// <para>Continues until the expected condition is satisfied or the <see cref="Configuration.Timeout"/> is reached.</para>
        /// </summary>
        /// <param name="locator">The text/value, name or id of the button</param>
        /// <param name="until">The condition to be satisfied</param>
        /// <param name="waitBetweenRetries">How long to wait for the condition to be satisfied before clicking again</param>
        /// <returns>The first matching button</returns>
        /// <exception cref="T:Coypu.MissingHtmlException">Thrown if the element cannot be found</exception>
        public DriverScope ClickButton(string locator, Func<bool> until, TimeSpan waitBetweenRetries)
        {
            TryUntil(() => ClickButton(locator), until, waitBetweenRetries);
            return this;
        }

        /// <summary>
        /// <para>Click a link and wait for a condition to be satisfied for a specified time otherwise click and wait again.</para> 
        /// <para>Continues until the expected condition is satisfied or the <see cref="Configuration.Timeout"/> is reached.</para>
        /// </summary>
        /// <param name="locator">The text of the link</param>
        /// <param name="until">The condition to be satisfied</param>
        /// <param name="waitBetweenRetries">How long to wait for the condition to be satisfied before clicking again</param>
        /// <returns>The first matching button</returns>
        /// <exception cref="T:Coypu.MissingHtmlException">Thrown if the element cannot be found</exception>
        public DriverScope ClickLink(string locator, Func<bool> until, TimeSpan waitBetweenRetries)
        {
            TryUntil(() => ClickLink(locator), until, waitBetweenRetries);
            return this;
        }

        /// <summary>
        /// Find the first input of type button|submit|image or div with the css class "button" to appear within the <see cref="Configuration.Timeout"/> .
        /// </summary>
        /// <param name="locator">The text/value, name or id of the button</param>
        /// <returns>A button</returns>
        /// <exception cref="T:Coypu.MissingHtmlException">Thrown if the element cannot be found</exception>
        public ElementScope FindButton(string locator)
        {
            return RetryUntilTimeout(() => driver.FindButton(locator));
        }

        /// <summary>
        /// Find the first matching link to appear within the <see cref="Configuration.Timeout"/>
        /// </summary>
        /// <param name="locator">The text of the link</param>
        /// <returns>A link</returns>
        /// <exception cref="T:Coypu.MissingHtmlException">Thrown if the element cannot be found</exception>
        public ElementScope FindLink(string locator)
        {
            return new ElementScope(new LinkFinder(driver, locator), this);
        }

        /// <summary>
        /// Find the first form field of any type to appear within the <see cref="Configuration.Timeout"/>
        /// </summary>
        /// <param name="locator">The text of the associated label element, the id or name, the placeholder text, the value of a radio button, the last part of the id (for asp.net forms testing)</param>
        /// <returns>A form field</returns>
        /// <exception cref="T:Coypu.MissingHtmlException">Thrown if the element cannot be found</exception>
        public ElementScope FindField(string locator)
        {
            return RetryUntilTimeout(() => driver.FindField(locator));
        }

        /// <summary>
        /// Find the first matching text field to appear within the <see cref="Configuration.Timeout"/> to fill in.
        /// </summary>
        /// <param name="locator">The text of the associated label element, the id or name, the placeholder text, the last part of the id (for asp.net forms testing)</param>
        /// <returns>With</returns>
        public FillInWith FillIn(string locator)
        {
            return new FillInWith(locator, driver, robustWrapper);
        }

        /// <summary>
        /// Fill in a previously found text field
        /// </summary>
        /// <param name="element">The text field</param>
        /// <returns>With</returns>
        public FillInWith FillIn(Element element)
        {
            return new FillInWith(element, driver, robustWrapper);
        }

        /// <summary>
        /// Select an option from a select element
        /// </summary>
        /// <param name="option">The text or value of the option to select</param>
        /// <returns>From</returns>
        public SelectFrom Select(string option)
        {
            return new SelectFrom(option, driver, robustWrapper);
        }

        /// <summary>
        /// CQuery whether text appears on the page within the <see cref="Configuration.Timeout"/>
        /// </summary>
        /// <param name="text">The exact text to find</param>
        /// <returns>Whether the text appears</returns>
        public bool HasContent(string text)
        {
            return Query(() => driver.HasContent(text), true);
        }

        /// <summary>
        /// Query whether text appears on the page using a regular expression within the <see cref="Configuration.Timeout"/>
        /// </summary>
        /// <param name="pattern">The regular expression to match</param>
        /// <returns>Whether the page text matches</returns>
        public bool HasContentMatch(Regex pattern)
        {
            return Query(() => driver.HasContentMatch(pattern), true);
        }

        /// <summary>
        /// Query whether text does not appear on the page. Returns as soon as the text does not appear, or when the <see cref="Configuration.Timeout"/> is reached.
        /// </summary>
        /// <param name="text">The exact text expected not to be found</param>
        /// <returns>Whether the text does not appear</returns>
        public bool HasNoContent(string text)
        {
            return !Query(() => driver.HasContent(text), false);
        }

        /// <summary>
        /// Query whether text does not appear on the page using a regular expression. Returns as soon as the text does not appear, or when the <see cref="Configuration.Timeout"/> is reached.
        /// </summary>
        /// <param name="pattern">The regular expression expected not to match</param>
        /// <returns>Whether the text does not appear</returns>
        public bool HasNoContentMatch(Regex pattern)
        {
            return !Query(() => driver.HasContentMatch(pattern), false);
        }

        /// <summary>
        /// Query whether an element matching a CSS selector appears on the page within the <see cref="Configuration.Timeout"/>
        /// </summary>
        /// <param name="cssSelector">CSS selector</param>
        /// <returns>Whether an element appears</returns>
        public bool HasCss(string cssSelector)
        {
            return Query(() => driver.HasCss(cssSelector), true);
        }

        /// <summary>
        /// Query whether an element matching a CSS selector does not appear on the page. Returns as soon as the element does not appear, or when the <see cref="Configuration.Timeout"/> is reached.
        /// </summary>
        /// <param name="cssSelector">CSS selector</param>
        /// <returns>Whether an element does not appear</returns>
        public bool HasNoCss(string cssSelector)
        {
            return !Query(() => driver.HasCss(cssSelector), false);
        }

        /// <summary>
        /// Query whether an element matching an XPath query appears on the page within the <see cref="Configuration.Timeout"/>
        /// </summary>
        /// <param name="xpath">XPath query</param>
        /// <returns>Whether an element appears</returns>
        public bool HasXPath(string xpath)
        {
            return Query(() => driver.HasXPath(xpath), true);
        }

        /// <summary>
        /// Query whether an element matching an XPath query appears on the page. Returns as soon as the element appears, or when the <see cref="Configuration.Timeout"/> is reached.
        /// </summary>
        /// <param name="xpath">XPath query</param>
        /// <returns>Whether an element appears</returns>
        public bool HasNoXPath(string xpath)
        {
            return !Query(() => driver.HasXPath(xpath), false);
        }

        /// <summary>
        /// Find an element matching a CSS selector
        /// </summary>
        /// <param name="cssSelector">CSS selector</param>
        /// <returns>The first matchin element</returns>
        public ElementScope FindCss(string cssSelector)
        {
            return RetryUntilTimeout(() => driver.FindCss(cssSelector));
        }

        /// <summary>
        /// Find an element matching an XPath query
        /// </summary>
        /// <param name="xpath">XPath query</param>
        /// <returns>The first matchin element</returns>
        public ElementScope FindXPath(string xpath)
        {
            return RetryUntilTimeout(() => driver.FindXPath(xpath));
        }

        /// <summary>
        /// Find all elements matching a CSS selector at the current moment. Does not wait until the <see cref="Configuration.Timeout"/> but returns as soon as the driver does.
        /// </summary>
        /// <param name="cssSelector">CSS selector</param>
        /// <returns>All matching elements</returns>
        public IEnumerable<Element> FindAllCss(string cssSelector)
        {
            return driver.FindAllCss(cssSelector);
        }

        /// <summary>
        /// Find all elements matching an XPath query at the current moment. Does not wait until the <see cref="Configuration.Timeout"/> but returns as soon as the driver does.
        /// </summary>
        /// <param name="xpath">XPath query</param>
        /// <returns>All matching elements</returns>
        public IEnumerable<Element> FindAllXPath(string xpath)
        {
            return driver.FindAllXPath(xpath);
        }

        /// <summary>
        /// <para>Find the first matching section to appear within the <see cref="Configuration.Timeout"/></para>
        /// <para>Sections are identified by the text of their child heading element, or by id.</para>
        /// <para>E.g. to find this:
        /// 
        /// <code>    
        ///     &lt;div&gt;	
        ///         &lt;h2&gt;Search results&lt;/h2&gt;
        ///         ...
        ///     &lt;/div&gt;</code>
        ///
        /// or this:
        ///
        /// <code>
        ///     &lt;section&gt;
        ///         &lt;h3&gt;Search results&lt;/h3&gt;
        ///         ...
        ///     &lt;/section&gt;</code>
        /// </para>
        /// <para>use this:</para>
        /// <para>
        /// <code>    FindSection("Search results")</code>
        /// </para>
        /// </summary>
        /// <param name="locator">The text of a child heading element or section id</param>
        /// <returns>An element</returns>
        /// <exception cref="T:Coypu.MissingHtmlException">Thrown if the element cannot be found</exception>
        public ElementScope FindSection(string locator)
        {
            return RetryUntilTimeout(() => driver.FindSection(locator));
        }

        /// <summary>
        /// <para>Find the first matching fieldset to appear within the <see cref="Configuration.Timeout"/></para>
        /// <para>Fieldsets are identified by the text of their child legend element, or by id.</para>
        /// <para>E.g. to find this:
        /// 
        /// <code>    
        ///     &lt;fieldset&gt;	
        ///         &lt;legend&gt;Advanced search&lt;/legend&gt;
        ///         ...
        ///     &lt;/fieldset&gt;</code>
        /// </para>
        /// <para>use this:</para>
        /// <para>
        /// <code>    FindFieldset("Advanced search")</code>
        /// </para>
        /// </summary>
        /// <param name="locator">The text of a child legend element or fieldset id</param>
        /// <returns>An element</returns>
        /// <exception cref="T:Coypu.MissingHtmlException">Thrown if the element cannot be found</exception>
        public ElementScope FindFieldset(string locator)
        {
            return RetryUntilTimeout(() => driver.FindFieldset(locator));
        }

        /// <summary>
        /// Find the first matching element with specified id to appear within the <see cref="Configuration.Timeout"/>
        /// </summary>
        /// <param name="id">Element id</param>
        /// <returns>An elemenet</returns>
        /// <exception cref="T:Coypu.MissingHtmlException">Thrown if the element cannot be found</exception>
        public ElementScope FindId(string id)
        {
            return RetryUntilTimeout(() => driver.FindId(id));
        }

        /// <summary>
        /// Check the first checkbox to appear within the <see cref="Configuration.Timeout"/> matching the text of the associated label element, the id, name or the last part of the id (for asp.net forms testing).
        /// </summary>
        /// <param name="locator">The text of the associated label element, the id or name, the last part of the id (for asp.net forms testing)</param>
        /// <exception cref="T:Coypu.MissingHtmlException">Thrown if the element cannot be found</exception>
        public DriverScope Check(string locator)
        {
            RetryUntilTimeout(() => driver.Check(driver.FindField(locator)));
        }

        /// <summary>
        /// Uncheck the first checkbox to appear within the <see cref="Configuration.Timeout"/> matching the text of the associated label element, the id, name or the last part of the id (for asp.net forms testing).
        /// </summary>
        /// <param name="locator">The text of the associated label element, the id or name, the last part of the id (for asp.net forms testing)</param>
        /// <exception cref="T:Coypu.MissingHtmlException">Thrown if the element cannot be found</exception>
        public DriverScope Uncheck(string locator)
        {
            RetryUntilTimeout(() => driver.Uncheck(driver.FindField(locator)));
        }

        /// <summary>
        /// Choose the first radio button to appear within the <see cref="Configuration.Timeout"/> matching the text of the associated label element, the id, the name, the value or the last part of the id (for asp.net forms testing).
        /// </summary>
        /// <param name="locator">The text of the associated label element, the id or name, the last part of the id (for asp.net forms testing)</param>
        /// <exception cref="T:Coypu.MissingHtmlException">Thrown if the element cannot be found</exception>
        public DriverScope Choose(string locator)
        {
            RetryUntilTimeout(() => driver.Choose(driver.FindField(locator)));
        }

        /// <summary>
        /// Executes custom javascript in the browser
        /// </summary>
        /// <param name="javascript">JavaScript to execute</param>
        /// <returns>Anything returned from the script</returns>
        public string ExecuteScript(string javascript)
        {
            return driver.ExecuteScript(javascript);
        }

        /// <summary>
        /// Hover the mouse over an element
        /// </summary>
        /// <param name="findElement">A function to find the element</param>
        public void Hover(Func<Element> findElement)
        {
            RetryUntilTimeout(() => driver.Hover(findElement()));
        }

        /// <summary>
        /// Query whether an element appears within the <see cref="Configuration.Timeout"/>
        /// </summary>
        /// <param name="findElement">A function to find an element</param>
        public bool Has(Func<Element> findElement) 
        {
            return Query(new ZeroTimeoutQueryBuilder().BuildZeroTimeoutHasElementQuery(findElement), true);
        }

        /// <summary>
        /// Query whether an element does not appear. Returns as soon as the element does not appear or after the <see cref="Configuration.Timeout"/>
        /// </summary>
        /// <param name="findElement">A function to find an element</param>
        public bool HasNo(Func<Element> findElement)
        {
            return !Query(new ZeroTimeoutQueryBuilder().BuildZeroTimeoutHasElementQuery(findElement), false);
        }

        /// <summary>
        /// <para>Retry an action on any exception until it succeeds. Once the <see cref="Configuration.Timeout"/> is passed any exception will be rethrown.</para>
        /// <para>Waits for the <see cref="Configuration.RetryInterval"/> between retries</para>
        /// </summary>
        /// <param name="action">An action</param>
        public void RetryUntilTimeout(Action action)
        {
            robustWrapper.Robustly(action);
        }

        /// <summary>
        /// <para>Retry a function on any exception until it succeeds. Once the <see cref="Configuration.Timeout"/> is passed any exception will be rethrown.</para>
        /// <para>Waits for the <see cref="Configuration.RetryInterval"/> between retries</para>
        /// </summary>
        /// <param name="function">A function</param>
        public TResult RetryUntilTimeout<TResult>(Func<TResult> function)
        {
            return robustWrapper.Robustly(function);
        }

        /// <summary>
        /// <para>Execute a query repeatedly until either the expected result is returned or the <see cref="Configuration.Timeout"/> is passed.</para>
        /// <para>Once the <see cref="Configuration.Timeout"/> is passed any result will be returned or any exception will be rethrown.</para>
        /// <para>Waits for the <see cref="Configuration.RetryInterval"/> between retries.</para>
        /// </summary>
        /// <param name="query">A query</param>
        /// <param name="expecting">Expected result</param>
        public T Query<T>(Func<T> query, T expecting)
        {
            return robustWrapper.Query(query, expecting);
        }

        /// <summary>
        /// <para>Execute an action repeatedly until a condition is met.</para>
        /// <para>Allows the time specified in <paramref name="waitBeforeRetry"/> for the <paramref name="until"/> condition to be met before each retry.</para>
        /// <para>Once the <see cref="Configuration.Timeout"/> is passed a Coypu.MissingHtmlException will be thrown.</para>
        /// </summary>
        /// <param name="tryThis">The action to try</param>
        /// <param name="until">The condition to be met</param>
        /// <param name="waitBeforeRetry">How long to wait for the condition</param>
        /// <exception cref="T:Coypu.MissingHtmlException">Thrown if the until condition is never met</exception>
        public void TryUntil(Action tryThis, Func<bool> until, TimeSpan waitBeforeRetry)
        {
            robustWrapper.TryUntil(tryThis, until, waitBeforeRetry);
        }

        /// <summary>
        /// <para>Find the first from a list of possible states that your page may arrive at.</para>
        /// <para>Returns as soon as any of the possible states is found.</para>
        /// <para>E.g.:</para>
        /// 
        /// <code>  
        ///  var signedIn = new State(() => browser.HasContent("Signed in as:"));
        ///  var signedOut = new State(() => browser.HasContent("Please sign in"));
        ///  
        ///  if (browser.FindState(signedIn,signedOut) == signedIn) 
        ///  {
        ///    browser.ClickLink("Sign out");
        ///  }
        ///  </code>
        ///  </summary>
        /// <param name="states">The possible states you are expecting</param>
        /// <returns></returns>
        public State FindState(params State[] states)
        {
            return stateFinder.FindState(states);
        }

        public DriverScope ConsideringInvisibleElements()
        {
            this.consideringInvisibleElements = true;
            return this;
        }


        public Element Now()
        {
            return elementFinder.Find();
        }
    }
}