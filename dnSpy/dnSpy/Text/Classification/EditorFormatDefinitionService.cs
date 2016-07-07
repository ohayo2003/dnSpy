﻿/*
    Copyright (C) 2014-2016 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using dnSpy.Text.MEF;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace dnSpy.Text.Classification {
	interface IEditorFormatDefinitionService {
		Lazy<EditorFormatDefinition, IEditorFormatMetadata>[] EditorFormatDefinitions { get; }
		Lazy<EditorFormatDefinition, IClassificationFormatMetadata>[] ClassificationFormatDefinitions { get; }
		EditorFormatDefinition GetDefinition(string key);
	}

	[Export(typeof(IEditorFormatDefinitionService))]
	sealed class EditorFormatDefinitionService : IEditorFormatDefinitionService {
		public Lazy<EditorFormatDefinition, IEditorFormatMetadata>[] EditorFormatDefinitions { get; }
		public Lazy<EditorFormatDefinition, IClassificationFormatMetadata>[] ClassificationFormatDefinitions { get; }
		readonly Dictionary<string, Lazy<EditorFormatDefinition, IEditorFormatMetadata>> toLazy;

		[ImportingConstructor]
		EditorFormatDefinitionService([ImportMany] IEnumerable<Lazy<EditorFormatDefinition, IEditorFormatMetadata>> editorFormatDefinitions, [ImportMany] IEnumerable<Lazy<EditorFormatDefinition, IClassificationFormatMetadata>> classificationFormatDefinitions) {
			EditorFormatDefinitions = editorFormatDefinitions.Where(a => Filter(a.Metadata.Name)).ToArray();
			ClassificationFormatDefinitions = Orderer.Order(classificationFormatDefinitions.Where(a => Filter(((IEditorFormatMetadata)a.Metadata).Name))).ToArray();
			this.toLazy = new Dictionary<string, Lazy<EditorFormatDefinition, IEditorFormatMetadata>>(StringComparer.OrdinalIgnoreCase);
			foreach (var e in EditorFormatDefinitions) {
				var name = e.Metadata.Name;
				if (!toLazy.ContainsKey(name))
					toLazy.Add(name, e);
			}
		}

		static bool Filter(string s) => s != Priority.Low && s != Priority.Default && s != Priority.High;

		public EditorFormatDefinition GetDefinition(string key) {
			Lazy<EditorFormatDefinition, IEditorFormatMetadata> lazy;
			if (!toLazy.TryGetValue(key, out lazy))
				return null;
			return lazy.Value;
		}
	}
}