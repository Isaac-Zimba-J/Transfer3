using System;

namespace Transfer3.Services.Contracts;

public interface IFilePickerService
{
    Task<string?> PickFileAsync();
    Task<List<string>> PickMultipleFileAsync();
    Task<string?> PickFolderAsync();

}
