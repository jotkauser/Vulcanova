﻿using System;

namespace Vulcanova.iOS.CollectionView
{
	public interface IItemsViewSource : IDisposable
	{
		int ItemCount { get; }
		int ItemCountInGroup(nint group);
		int GroupCount { get; }
		object this[Foundation.NSIndexPath indexPath] { get; }
		object Group(Foundation.NSIndexPath indexPath);
		Foundation.NSIndexPath GetIndexForItem(object item);
	}
}