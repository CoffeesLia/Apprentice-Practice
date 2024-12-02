

using Application.ViewModel;

namespace Domain.ViewModel
{
    public class PartNumberFilterVM : BaseFilterVM
    {
        public string? Code { get; set; }
        public string? Description { get; set; }

        public int? Type { get; set; }

    }
}
