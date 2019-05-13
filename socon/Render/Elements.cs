using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socon.Render
{
	public interface IRenderer : Base.IDXRebasable
	{
		void Render();
	}
	public static class Elements
	{
		public static List<IRenderer> AllElements = new List<IRenderer>();

		public static void Add(IRenderer el)
		{
			lock (AllElements)
				AllElements.Add(el);

			if (el.GetType().GetInterfaces().Contains(typeof(Keyboard.IKeyboardInputReceiver)))
				Base.CurrentKeyboardInput.SwitchReceiver((Keyboard.IKeyboardInputReceiver)el);
		}

		public static void AddBefore<T>(IRenderer el)
		{
			lock (AllElements) {
				for (int x = 0; x < AllElements.Count; x++) {
					if (AllElements[x] is T) {
						AllElements.Insert(x, el);
						return;
					}
				}
			}
			Add(el);
		}

		public static void AddAfter<T>(IRenderer el)
		{
			lock (AllElements) {
				int last = AllElements.Count - 1;
				for (int x = 0; x < AllElements.Count; x++) {
					if (AllElements[x] is T)
						last = x;
				}
				AllElements.Insert(last + 1, el);
			}
		}

		public static IRenderer Replace<T>(T el) where T : IRenderer
		{
			lock (AllElements) {
				var foundEl = AllElements.FirstOrDefault(x => x is T);
				if (foundEl == null)
					return default(T);
				AllElements.Remove(foundEl);
				Add(el);
				return foundEl;
			}
		}

		public static void Remove<T>() where T : IRenderer
		{
			lock (AllElements) {
				AllElements.RemoveAll(x => x is T);

				for (int x = AllElements.Count - 1;x >= 0;x--) {
					if (AllElements[x].GetType().GetInterfaces().Contains(typeof(Keyboard.IKeyboardInputReceiver))) {
						Base.CurrentKeyboardInput.SwitchReceiver((Keyboard.IKeyboardInputReceiver)AllElements[x]);
						break;
					}
				}
			}
		}

		public static void Remove(IRenderer el)
		{
			lock (AllElements) {
				AllElements.Remove(el);

				for (int x = AllElements.Count - 1; x >= 0; x--) {
					if (AllElements[x].GetType().GetInterfaces().Contains(typeof(Keyboard.IKeyboardInputReceiver))) {
						Base.CurrentKeyboardInput.SwitchReceiver((Keyboard.IKeyboardInputReceiver)AllElements[x]);
						break;
					}
				}
			}
		}

		public static bool Exists<T>() where T : IRenderer
		{
			lock (AllElements)
				return AllElements.FirstOrDefault(x => x is T) != null;
		}

		public static bool Exists(IRenderer el)
		{
			lock (AllElements)
				return AllElements.Contains(el);
		}
	}
}
