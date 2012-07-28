﻿namespace IronPigeon.Tests.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class LoggerMock : ILogger {
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private List<string> messages = new List<string>();

		internal IReadOnlyList<string> Messages {
			get { return this.messages; }
		}

		public void WriteLine(string message) {
			this.messages.Add(message);
			Trace.WriteLine(message);
			Console.WriteLine(message);
		}

		public override string ToString() {
			var builder = new StringBuilder();
			foreach (var message in this.messages) {
				builder.AppendLine(message);
			}

			return builder.ToString();
		}
	}
}
