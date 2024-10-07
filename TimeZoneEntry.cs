using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace WorldClockStatusWindow;

public class TimeZoneEntry : INotifyDataErrorInfo
{
    private readonly DataErrorDictionary _dataErrorDictionary;

    public TimeZoneEntry()
    {
        _dataErrorDictionary = new DataErrorDictionary();
        _dataErrorDictionary.ErrorsChanged += DataErrorDictionaryErrorsChanged;
    }

    private string _label;

    public string Label
    {
        get => _label;
        set
        {
            if (!ValidateLabel(value))
                return;

            _label = value;
        }
    }

    public string TimeZoneId { get; set; }

    [JsonIgnore]
    public bool HasErrors => _dataErrorDictionary.Any();

    [JsonIgnore]
    public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);

    public IEnumerable GetErrors(string propertyName)
    {
        return _dataErrorDictionary.GetErrors(propertyName);
    }

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    private void DataErrorDictionaryErrorsChanged(object sender, DataErrorsChangedEventArgs e)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(e.PropertyName));
    }

    private bool ValidateLabel(string newValue)
    {
        _dataErrorDictionary.ClearErrors(nameof(Label));

        if (!string.IsNullOrWhiteSpace(newValue))
            return true;

        _dataErrorDictionary.AddError(nameof(Label), "Label cannot be empty");

        return false;
    }
}