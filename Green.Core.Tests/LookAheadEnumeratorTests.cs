using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Green.Tests
{
	public sealed class LookAheadEnumeratorTests
	{
		[Fact]
		public void Next()
		{
			var x = LookAhead.Enumerate(new int[] { 10, 15 });
			Assert.Equal(10, x.Next(0));
			Assert.Equal(15, x.Next(1));
			Assert.Throws<InvalidOperationException>(() => x.Next(2));
			Assert.True(x.HasNext(0));
			Assert.True(x.HasNext(1));
			Assert.False(x.HasNext(2));
		}

		[Fact]
		public void Consume()
		{
			var x = LookAhead.Enumerate(new int[] { 10, 15 });
			Assert.Equal(10, x.Next(0));
			Assert.Equal(15, x.Next(1));
			Assert.Throws<InvalidOperationException>(() => x.Next(2));
			x.Advance(1);
			Assert.Equal(15, x.Next(0));
			Assert.Throws<InvalidOperationException>(() => x.Next(1));
			Assert.Throws<InvalidOperationException>(() => x.Advance(2));
		}

		[Fact]
		public void Advance_NumberZero_ChangesNothing()
		{
			var x = LookAhead.Enumerate(new int[] { 10 });
			Assert.Equal(10, x.Next());
			Assert.False(x.HasNext(1));
			x.Advance(0);
			Assert.Equal(10, x.Next());
			Assert.False(x.HasNext(1));
			x.Advance(1);
			Assert.False(x.HasNext());
			x.Advance(0);
			Assert.False(x.HasNext());
		}

		[Fact]
		public void BigList()
		{
			var x = LookAhead.Enumerate(new int[] { 10, 15, 20 });
			Assert.Equal(10, x.Next(0));
			Assert.Equal(15, x.Next(1));
			Assert.True(x.HasNext(0));
			Assert.True(x.HasNext(1));

			x.Advance(1);
			Assert.Equal(15, x.Next(0));
			Assert.Equal(20, x.Next(1));
			Assert.True(x.HasNext(0));
			Assert.True(x.HasNext(1));

			x.Advance(1);
			Assert.Equal(20, x.Next(0));
			Assert.Throws<InvalidOperationException>(() => x.Next(1));
			Assert.True(x.HasNext(0));
			Assert.False(x.HasNext(1));

			x.Advance(1);
			Assert.Throws<InvalidOperationException>(() => x.Next(0));
			Assert.Throws<InvalidOperationException>(() => x.Next(1));
			Assert.False(x.HasNext(0));
			Assert.False(x.HasNext(1));
		}
	}
}
