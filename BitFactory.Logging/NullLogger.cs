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

namespace BitFactory.Logging
{
	/// <summary>
	/// Instances of this class can be used as placeholders where the actual
	/// logger is not yet known.
	/// Notice that the only difference between this class and its superclass
	/// is that this class is not abstract.
	/// </summary>
	/// <remarks>
	/// Since the only difference between this class and its superclass, Logger,
	/// is that Logger is abstract whereas this class isn't,
	/// one might ask the question, "Why not just make Logger a concrete class
	/// that can be used as a 'null' logger instead of creating a new subclass?"
	/// The answer is that Logger, from a design point of view, *is* abstract,
	/// and shouldn't be changed just to accomodate this functionality. Also,
	/// by using a NullLogger in code, an explicit design declaration is made that
	/// indicates its purpose as being a "null" logger.
	/// </remarks>
	public class NullLogger : Logger
	{
	}
}
