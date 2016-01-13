﻿/*
The MIT License (MIT)

Copyright (c) 2015 Objectivity Bespoke Software Specialists

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace Objectivity.Test.Automation.Tests.NUnit.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    using global::NUnit.Framework;

    /// <summary>
    /// DataDriven methods for NUnit test framework
    /// </summary>
    public static class DataDrivenHelper
    {
        /// <summary>
        /// Reads the data drive file and set test name.
        /// </summary>
        /// <param name="folder">Path to folder with DataDriveFile</param>
        /// <param name="testData">Name of the child element in xml file.</param>
        /// <param name="diffParam">The difference parameter, will be used in test case name.</param>
        /// <param name="testName">Name of the test.</param>
        /// <returns></returns>
        public static IEnumerable<TestCaseData> ReadDataDriveFile(string folder, string testData, string[] diffParam, [Optional] string testName)
        {
            var doc = XDocument.Load(folder);

            if (!doc.Descendants(testData).Any())
            {
                throw new ArgumentNullException(string.Format(" Exception while reading Data Driven file\n row '{0}' not found \n in file '{1}'", testData, folder));
            }

            foreach (XElement element in doc.Descendants(testData))
            {
                var testParams = element.Attributes().ToDictionary(k => k.Name.ToString(), v => v.Value);

                var testCaseName = string.IsNullOrEmpty(testName) ? testData : testName;
                try
                {
                    testCaseName = TestCaseName(diffParam, testParams, testCaseName);
                }
                catch (NullReferenceException e)
                {
                    throw new NullReferenceException(
                        string.Format(
                            " Exception while reading Data Driven file\n test data '{0}' \n test name '{1}' \n searched key '{2}' not found in row \n '{3}'  \n in file '{4}'",
                            testData,
                            testName,
                            e.Message,
                            element,
                            folder));
                }

                var data = new TestCaseData(testParams);
                data.SetName(testCaseName);
                yield return data;
            }
        }

        /// <summary>
        /// Reads the data drive file without setting test name.
        /// </summary>
        /// <param name="folder">Path to folder with DataDriven file</param>
        /// <param name="testData">The test data.</param>
        /// <returns></returns>
        public static IEnumerable<TestCaseData> ReadDataDriveFile(string folder, string testData)
        {
            var doc = XDocument.Load(folder);
            if (!doc.Descendants(testData).Any())
            {
                throw new ArgumentNullException(string.Format(CultureInfo.CurrentCulture, "Exception while reading Data Driven file\n row '{0}' not found \n in file '{1}'", testData, folder));
            }

            return doc.Descendants(testData).Select(element => element.Attributes().ToDictionary(k => k.Name.ToString(), v => v.Value)).Select(testParams => new TestCaseData(testParams));
        }

        /// <summary>
        /// Get the name of test case from value of parameters.
        /// </summary>
        /// <param name="diffParam">The difference parameter.</param>
        /// <param name="testParams">The test parameters.</param>
        /// <param name="testCaseName">Name of the test case.</param>
        /// <returns>Test case name</returns>
        /// <exception cref="NullReferenceException"></exception>
        private static string TestCaseName(string[] diffParam, Dictionary<string, string> testParams, string testCaseName)
        {
            if (diffParam != null && diffParam.Any())
            {
                foreach (var p in diffParam)
                {
                    string keyValue;
                    bool keyFlag = testParams.TryGetValue(p, out keyValue);

                    if (keyFlag)
                    {
                        if (!string.IsNullOrEmpty(keyValue))
                        {
                            testCaseName += "_" + Regex.Replace(keyValue, @"[.]+|\s+", "_");
                        }
                    }
                    else
                    {
                        throw new NullReferenceException(p);
                    }
                }
            }
            return testCaseName;
        }

    }
}
