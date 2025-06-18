using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olimpo;

public interface ILoadableViewModel
{
    Task LoadAsync(IDictionary<string, object>? parameters = null);
}
