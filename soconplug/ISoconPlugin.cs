using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socon.Plugins
{
	public interface ISoconPlugin
	{
		void Init();

		void Unload();

		string Name { get; }
	}
}
