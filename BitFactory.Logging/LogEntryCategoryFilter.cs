/* 
 * (C) Copyright 2002, 2009 - Lorne Brinkman - All Rights Reserved.
 * http://www.TheObjectGuy.com
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
 * OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 * 
 */

using System;
using System.Collections;

namespace BitFactory.Logging
{
	/// <summary>
	/// Instances filter LogEntries based on their categories.
	/// </summary>
	/// <remarks>
	/// By default, this is an "allow" filter, meaning that the specified categories
	/// contained herein pass through the filter. If allow is set to false, then all
	/// categories that are NOT contained herein will pass through.
	/// </remarks>
	public class LogEntryCategoryFilter : LogEntryFilter 
	{
		/// <summary>
		/// A Collection of categories.
		/// </summary>
		private IList _categories = new ArrayList();
		/// <summary>
		/// A flag to determine whether the categories are allowed or not.
		/// </summary>
		private bool _allow = true;

		/// <summary>
		/// Gets and sets the Allow flag.
		/// </summary>
		public bool Allow 
		{
			get { return _allow; }
			set { _allow = value; }
		}
		/// <summary>
		/// Gets and sets the categories.
		/// </summary>
		protected IList Categories 
		{
			get { return _categories; }
			set { _categories = value; }
		}
			
		/// <summary>
		/// Create a new instance of LogEntryCategoryFilter.
		/// </summary>
		protected LogEntryCategoryFilter() 
		{
		}
		/// <summary>
		/// Create a new instance of LogEntryCategoryFilter.
		/// </summary>
		/// <param name="allowFlag">If true, allow the categories. If false, disallow them.</param>
		public LogEntryCategoryFilter(bool allowFlag) : this()
		{
			Allow = allowFlag;
		}

		/// <summary>
		/// Add a category to the filter.
		/// </summary>
		/// <param name="aCategory">A category to add.</param>
		public void AddCategory(Object aCategory) 
		{
			Categories.Add(aCategory);
		}
		/// <summary>
		/// Remove a category from the filter.
		/// </summary>
		/// <param name="aCategory">The category to remove.</param>
		public void removeCategory(Object aCategory) 
		{
			Categories.Remove(aCategory);
		}
		/// <summary>
		/// Determine if a LogEntry "passes" through the filter.
		/// </summary>
		/// <param name="aLogEntry">The LogEntry being tested.</param>
		/// <returns>true if aLogEntry passes, false otherwise.</returns>
		protected override bool CanPass(LogEntry aLogEntry) 
		{
			bool hasCategory = Categories.Contains(aLogEntry.Category);
			return Allow ? hasCategory : ! hasCategory;
		}
	}
}
