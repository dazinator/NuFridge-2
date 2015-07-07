using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Management;

namespace NuFridge.Shared.Server.Diagnostics.Win {
    // Functions ShouldSerialize<PropertyName> are functions used by VS property browser to check if a particular property has to be serialized. These functions are added for all ValueType properties ( properties of type Int32, BOOL etc.. which cannot be set to null). These functions use Is<PropertyName>Null function. These functions are also used in the TypeConverter implementation for the properties to check for NULL value of property so that an empty value can be shown in Property browser in case of Drag and Drop in Visual studio.
    // Functions Is<PropertyName>Null() are used to check if a property is NULL.
    // Functions Reset<PropertyName> are added for Nullable Read/Write properties. These functions are used by VS designer in property browser to set a property to NULL.
    // Every property added to the class for WMI property has attributes set to define its behavior in Visual Studio designer and also to define a TypeConverter to be used.
    // Datetime conversion functions ToDateTime and ToDmtfDateTime are added to the class to convert DMTF datetime to System.DateTime and vice-versa.
    // An Early Bound class generated for the WMI class.Win32_Process
    public class Win32Process : Component {
        
        // Private property to hold the WMI namespace in which the class resides.
        private static string _createdWmiNamespace = "root\\cimv2";
        
        // Private property to hold the name of WMI class which created this class.
        private static string _createdClassName = "Win32_Process";
        
        // Private member variable to hold the ManagementScope which is used by the various methods.
        private static ManagementScope _statMgmtScope;
        
        private ManagementSystemProperties _privateSystemProperties;
        
        // Underlying lateBound WMI object.
        private ManagementObject _privateLateBoundObject;
        
        // Member variable to store the 'automatic commit' behavior for the class.
        private bool _autoCommitProp;
        
        // Private variable to hold the embedded property representing the instance.
        private ManagementBaseObject _embeddedObj;
        
        // The current WMI object used
        private ManagementBaseObject _curObj;
        
        // Flag to indicate if the instance is an embedded object.
        private bool _isEmbedded;
        
        // Below are different overloads of constructors to initialize an instance of the class with a WMI object.
        public Win32Process() {
            InitializeObject(null, null, null);
        }
        
        public Win32Process(string keyHandle) {
            InitializeObject(null, new ManagementPath(ConstructPath(keyHandle)), null);
        }
        
        public Win32Process(ManagementScope mgmtScope, string keyHandle) {
            InitializeObject(mgmtScope, new ManagementPath(ConstructPath(keyHandle)), null);
        }
        
        public Win32Process(ManagementPath path, ObjectGetOptions getOptions) {
            InitializeObject(null, path, getOptions);
        }
        
        public Win32Process(ManagementScope mgmtScope, ManagementPath path) {
            InitializeObject(mgmtScope, path, null);
        }
        
        public Win32Process(ManagementPath path) {
            InitializeObject(null, path, null);
        }
        
        public Win32Process(ManagementScope mgmtScope, ManagementPath path, ObjectGetOptions getOptions) {
            InitializeObject(mgmtScope, path, getOptions);
        }
        
        public Win32Process(ManagementObject theObject) {
            Initialize();
            if (CheckIfProperClass(theObject)) {
                _privateLateBoundObject = theObject;
                _privateSystemProperties = new ManagementSystemProperties(_privateLateBoundObject);
                _curObj = _privateLateBoundObject;
            }
            else {
                throw new ArgumentException("Class name does not match.");
            }
        }
        
        public Win32Process(ManagementBaseObject theObject) {
            Initialize();
            if (CheckIfProperClass(theObject)) {
                _embeddedObj = theObject;
                _privateSystemProperties = new ManagementSystemProperties(theObject);
                _curObj = _embeddedObj;
                _isEmbedded = true;
            }
            else {
                throw new ArgumentException("Class name does not match.");
            }
        }
        
        // Property returns the namespace of the WMI class.
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string OriginatingNamespace {
            get {
                return "root\\cimv2";
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ManagementClassName {
            get {
                string strRet = _createdClassName;
                if ((_curObj != null)) {
                    if ((_curObj.ClassPath != null)) {
                        strRet = ((string)(_curObj["__CLASS"]));
                        if (((strRet == null) 
                                    || (strRet == string.Empty))) {
                            strRet = _createdClassName;
                        }
                    }
                }
                return strRet;
            }
        }
        
        // Property pointing to an embedded object to get System properties of the WMI object.
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ManagementSystemProperties SystemProperties {
            get {
                return _privateSystemProperties;
            }
        }
        
        // Property returning the underlying lateBound object.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ManagementBaseObject LateBoundObject {
            get {
                return _curObj;
            }
        }
        
        // ManagementScope of the object.
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ManagementScope Scope {
            get
            {
                if ((_isEmbedded == false)) {
                    return _privateLateBoundObject.Scope;
                }
                return null;
            }
            set {
                if ((_isEmbedded == false)) {
                    _privateLateBoundObject.Scope = value;
                }
            }
        }
        
        // Property to show the commit behavior for the WMI object. If true, WMI object will be automatically saved after each property modification.(ie. Put() is called after modification of a property).
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AutoCommit {
            get {
                return _autoCommitProp;
            }
            set {
                _autoCommitProp = value;
            }
        }
        
        // The ManagementPath of the underlying WMI object.
        [Browsable(true)]
        public ManagementPath Path {
            get
            {
                if ((_isEmbedded == false)) {
                    return _privateLateBoundObject.Path;
                }
                return null;
            }
            set {
                if ((_isEmbedded == false)) {
                    if ((CheckIfProperClass(null, value, null) != true)) {
                        throw new ArgumentException("Class name does not match.");
                    }
                    _privateLateBoundObject.Path = value;
                }
            }
        }
        
        // Public static scope property which is used by the various methods.
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static ManagementScope StaticScope {
            get {
                return _statMgmtScope;
            }
            set {
                _statMgmtScope = value;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Caption property is a short textual description (one-line string) of the obje" +
            "ct.")]
        public string Caption {
            get {
                return ((string)(_curObj["Caption"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The CommandLine property specifies the command line used to start a particular pr" +
            "ocess, if applicable.")]
        public string CommandLine {
            get {
                return ((string)(_curObj["CommandLine"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("CreationClassName indicates the name of the class or the subclass used in the cre" +
            "ation of an instance. When used with the other key properties of this class, thi" +
            "s property allows all instances of this class and its subclasses to be uniquely " +
            "identified.")]
        public string CreationClassName {
            get {
                return ((string)(_curObj["CreationClassName"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsCreationDateNull {
            get
            {
                if ((_curObj["CreationDate"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Time that the process began executing.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public DateTime CreationDate {
            get
            {
                if ((_curObj["CreationDate"] != null)) {
                    return ToDateTime(((string)(_curObj["CreationDate"])));
                }
                return DateTime.MinValue;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("CSCreationClassName contains the scoping computer system\'s creation class name.")]
        public string CsCreationClassName {
            get {
                return ((string)(_curObj["CSCreationClassName"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The scoping computer system\'s name.")]
        public string CsName {
            get {
                return ((string)(_curObj["CSName"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Description property provides a textual description of the object. ")]
        public string Description {
            get {
                return ((string)(_curObj["Description"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ExecutablePath property indicates the path to the executable file of the proc" +
            "ess.\nExample: C:\\WINDOWS\\EXPLORER.EXE")]
        public string ExecutablePath {
            get {
                return ((string)(_curObj["ExecutablePath"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsExecutionStateNull {
            get
            {
                if ((_curObj["ExecutionState"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Indicates the current operating condition of the process. Values include ready (2" +
            "), running (3), and blocked (4), among others.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public ExecutionStateValues ExecutionState {
            get {
                if ((_curObj["ExecutionState"] == null)) {
                    return ((ExecutionStateValues)(Convert.ToInt32(10)));
                }
                return ((ExecutionStateValues)(Convert.ToInt32(_curObj["ExecutionState"])));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("A string used to identify the process. A process ID is a kind of process handle.")]
        public string Handle {
            get {
                return ((string)(_curObj["Handle"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsHandleCountNull {
            get
            {
                if ((_curObj["HandleCount"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The HandleCount property specifies the total number of handles currently open by this process. This number is the sum of the handles currently open by each thread in this process. A handle is used to examine or modify the system resources. Each handle has an entry in an internally maintained table. These entries contain the addresses of the resources and the means to identify the resource type.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public uint HandleCount {
            get {
                if ((_curObj["HandleCount"] == null)) {
                    return Convert.ToUInt32(0);
                }
                return ((uint)(_curObj["HandleCount"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsInstallDateNull {
            get
            {
                if ((_curObj["InstallDate"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The InstallDate property is datetime value indicating when the object was install" +
            "ed. A lack of a value does not indicate that the object is not installed.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public DateTime InstallDate {
            get
            {
                if ((_curObj["InstallDate"] != null)) {
                    return ToDateTime(((string)(_curObj["InstallDate"])));
                }
                return DateTime.MinValue;
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsKernelModeTimeNull {
            get
            {
                if ((_curObj["KernelModeTime"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Time in kernel mode, in 100 nanoseconds. If this information is not available, a " +
            "value of 0 should be used.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public ulong KernelModeTime {
            get {
                if ((_curObj["KernelModeTime"] == null)) {
                    return Convert.ToUInt64(0);
                }
                return ((ulong)(_curObj["KernelModeTime"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsMaximumWorkingSetSizeNull {
            get
            {
                if ((_curObj["MaximumWorkingSetSize"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The MaximumWorkingSetSize property indicates the maximum working set size of the process. The working set of a process is the set of memory pages currently visible to the process in physical RAM. These pages are resident and available for an application to use without triggering a page fault.
Example: 1413120.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public uint MaximumWorkingSetSize {
            get {
                if ((_curObj["MaximumWorkingSetSize"] == null)) {
                    return Convert.ToUInt32(0);
                }
                return ((uint)(_curObj["MaximumWorkingSetSize"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsMinimumWorkingSetSizeNull {
            get
            {
                if ((_curObj["MinimumWorkingSetSize"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The MinimumWorkingSetSize property indicates the minimum working set size of the process. The working set of a process is the set of memory pages currently visible to the process in physical RAM. These pages are resident and available for an application to use without triggering a page fault.
Example: 20480.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public uint MinimumWorkingSetSize {
            get {
                if ((_curObj["MinimumWorkingSetSize"] == null)) {
                    return Convert.ToUInt32(0);
                }
                return ((uint)(_curObj["MinimumWorkingSetSize"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Name property defines the label by which the object is known. When subclassed" +
            ", the Name property can be overridden to be a Key property.")]
        public string Name {
            get {
                return ((string)(_curObj["Name"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The scoping operating system\'s creation class name.")]
        public string OsCreationClassName {
            get {
                return ((string)(_curObj["OSCreationClassName"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The scoping operating system\'s name.")]
        public string OsName {
            get {
                return ((string)(_curObj["OSName"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsOtherOperationCountNull {
            get
            {
                if ((_curObj["OtherOperationCount"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The OtherOperationCount property specifies the number of I/O operations performed" +
            ", other than read and write operations.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public ulong OtherOperationCount {
            get {
                if ((_curObj["OtherOperationCount"] == null)) {
                    return Convert.ToUInt64(0);
                }
                return ((ulong)(_curObj["OtherOperationCount"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsOtherTransferCountNull {
            get
            {
                if ((_curObj["OtherTransferCount"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The OtherTransferCount property specifies the amount of data transferred during o" +
            "perations other than read and write operations.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public ulong OtherTransferCount {
            get {
                if ((_curObj["OtherTransferCount"] == null)) {
                    return Convert.ToUInt64(0);
                }
                return ((ulong)(_curObj["OtherTransferCount"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPageFaultsNull {
            get
            {
                if ((_curObj["PageFaults"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The PageFaults property indicates the number of page faults generated by the proc" +
            "ess.\nExample: 10")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public uint PageFaults {
            get {
                if ((_curObj["PageFaults"] == null)) {
                    return Convert.ToUInt32(0);
                }
                return ((uint)(_curObj["PageFaults"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPageFileUsageNull {
            get
            {
                if ((_curObj["PageFileUsage"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The PageFileUsage property indicates the amountof page file space currently being" +
            " used by the process.\nExample: 102435")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public uint PageFileUsage {
            get {
                if ((_curObj["PageFileUsage"] == null)) {
                    return Convert.ToUInt32(0);
                }
                return ((uint)(_curObj["PageFileUsage"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsParentProcessIdNull {
            get
            {
                if ((_curObj["ParentProcessId"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The ParentProcessId property specifies the unique identifier of the process that created this process. Process identifier numbers are reused, so they only identify a process for the lifetime of that process. It is possible that the process identified by ParentProcessId has terminated, so ParentProcessId may not refer to an running process. It is also possible that ParentProcessId incorrectly refers to a process which re-used that process identifier. The CreationDate property can be used to determine whether the specified parent was created after this process was created.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public uint ParentProcessId {
            get {
                if ((_curObj["ParentProcessId"] == null)) {
                    return Convert.ToUInt32(0);
                }
                return ((uint)(_curObj["ParentProcessId"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPeakPageFileUsageNull {
            get
            {
                if ((_curObj["PeakPageFileUsage"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The PeakPageFileUsage property indicates the maximum amount of page file space  u" +
            "sed during the life of the process.\nExample: 102367")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public uint PeakPageFileUsage {
            get {
                if ((_curObj["PeakPageFileUsage"] == null)) {
                    return Convert.ToUInt32(0);
                }
                return ((uint)(_curObj["PeakPageFileUsage"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPeakVirtualSizeNull {
            get
            {
                if ((_curObj["PeakVirtualSize"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The PeakVirtualSize property specifies the maximum virtual address space the process has used at any one time. Use of virtual address space does not necessarily imply corresponding use of either disk or main memory pages. However, virtual space is finite, and by using too much, the process might limit its ability to load libraries.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public ulong PeakVirtualSize {
            get {
                if ((_curObj["PeakVirtualSize"] == null)) {
                    return Convert.ToUInt64(0);
                }
                return ((ulong)(_curObj["PeakVirtualSize"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPeakWorkingSetSizeNull {
            get
            {
                if ((_curObj["PeakWorkingSetSize"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The PeakWorkingSetSize property indicates the peak working set size of the proces" +
            "s.\nExample: 1413120")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public uint PeakWorkingSetSize {
            get {
                if ((_curObj["PeakWorkingSetSize"] == null)) {
                    return Convert.ToUInt32(0);
                }
                return ((uint)(_curObj["PeakWorkingSetSize"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPriorityNull {
            get
            {
                if ((_curObj["Priority"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Priority property indicates the scheduling priority of the process within the" +
            " operating system. The higher the value, the higher priority the process receive" +
            "s. Priority values can range from 0 (lowest priority) to 31 (highest priority).\n" +
            "Example: 7.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public uint Priority {
            get {
                if ((_curObj["Priority"] == null)) {
                    return Convert.ToUInt32(0);
                }
                return ((uint)(_curObj["Priority"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPrivatePageCountNull {
            get
            {
                if ((_curObj["PrivatePageCount"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The PrivatePageCount property specifies the current number of pages allocated tha" +
            "t are accessible only to this process ")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public ulong PrivatePageCount {
            get {
                if ((_curObj["PrivatePageCount"] == null)) {
                    return Convert.ToUInt64(0);
                }
                return ((ulong)(_curObj["PrivatePageCount"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsProcessIdNull {
            get
            {
                if ((_curObj["ProcessId"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ProcessId property contains the global process identifier that can be used to" +
            " identify a process. The value is valid from the creation of the process until t" +
            "he process is terminated.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public uint ProcessId {
            get {
                if ((_curObj["ProcessId"] == null)) {
                    return Convert.ToUInt32(0);
                }
                return ((uint)(_curObj["ProcessId"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsQuotaNonPagedPoolUsageNull {
            get
            {
                if ((_curObj["QuotaNonPagedPoolUsage"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The QuotaNonPagedPoolUsage property indicates the quota amount of non-paged pool " +
            "usage for the process.\nExample: 15")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public uint QuotaNonPagedPoolUsage {
            get {
                if ((_curObj["QuotaNonPagedPoolUsage"] == null)) {
                    return Convert.ToUInt32(0);
                }
                return ((uint)(_curObj["QuotaNonPagedPoolUsage"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsQuotaPagedPoolUsageNull {
            get
            {
                if ((_curObj["QuotaPagedPoolUsage"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The QuotaPagedPoolUsage property indicates the quota amount of paged pool usage f" +
            "or the process.\nExample: 22")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public uint QuotaPagedPoolUsage {
            get {
                if ((_curObj["QuotaPagedPoolUsage"] == null)) {
                    return Convert.ToUInt32(0);
                }
                return ((uint)(_curObj["QuotaPagedPoolUsage"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsQuotaPeakNonPagedPoolUsageNull {
            get
            {
                if ((_curObj["QuotaPeakNonPagedPoolUsage"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The QuotaPeakNonPagedPoolUsage property indicates the peak quota amount of non-pa" +
            "ged pool usage for the process.\nExample: 31")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public uint QuotaPeakNonPagedPoolUsage {
            get {
                if ((_curObj["QuotaPeakNonPagedPoolUsage"] == null)) {
                    return Convert.ToUInt32(0);
                }
                return ((uint)(_curObj["QuotaPeakNonPagedPoolUsage"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsQuotaPeakPagedPoolUsageNull {
            get
            {
                if ((_curObj["QuotaPeakPagedPoolUsage"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The QuotaPeakPagedPoolUsage property indicates the peak quota amount of paged poo" +
            "l usage for the process.\n Example: 31")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public uint QuotaPeakPagedPoolUsage {
            get {
                if ((_curObj["QuotaPeakPagedPoolUsage"] == null)) {
                    return Convert.ToUInt32(0);
                }
                return ((uint)(_curObj["QuotaPeakPagedPoolUsage"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsReadOperationCountNull {
            get
            {
                if ((_curObj["ReadOperationCount"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ReadOperationCount property specifies the number of read operations performed" +
            ".")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public ulong ReadOperationCount {
            get {
                if ((_curObj["ReadOperationCount"] == null)) {
                    return Convert.ToUInt64(0);
                }
                return ((ulong)(_curObj["ReadOperationCount"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsReadTransferCountNull {
            get
            {
                if ((_curObj["ReadTransferCount"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ReadTransferCount property specifies the amount of data read.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public ulong ReadTransferCount {
            get {
                if ((_curObj["ReadTransferCount"] == null)) {
                    return Convert.ToUInt64(0);
                }
                return ((ulong)(_curObj["ReadTransferCount"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSessionIdNull {
            get
            {
                if ((_curObj["SessionId"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The SessionId property specifies the unique identifier that is generated by the o" +
            "perating system when the session is created. A session spans a period of time fr" +
            "om log in to log out on a particular system.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public uint SessionId {
            get {
                if ((_curObj["SessionId"] == null)) {
                    return Convert.ToUInt32(0);
                }
                return ((uint)(_curObj["SessionId"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The Status property is a string indicating the current status of the object. Various operational and non-operational statuses can be defined. Operational statuses are ""OK"", ""Degraded"" and ""Pred Fail"". ""Pred Fail"" indicates that an element may be functioning properly but predicting a failure in the near future. An example is a SMART-enabled hard drive. Non-operational statuses can also be specified. These are ""Error"", ""Starting"", ""Stopping"" and ""Service"". The latter, ""Service"", could apply during mirror-resilvering of a disk, reload of a user permissions list, or other administrative work. Not all such work is on-line, yet the managed element is neither ""OK"" nor in one of the other states.")]
        public string Status {
            get {
                return ((string)(_curObj["Status"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsTerminationDateNull {
            get
            {
                if ((_curObj["TerminationDate"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Time that the process was stopped or terminated.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public DateTime TerminationDate {
            get
            {
                if ((_curObj["TerminationDate"] != null)) {
                    return ToDateTime(((string)(_curObj["TerminationDate"])));
                }
                return DateTime.MinValue;
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsThreadCountNull {
            get
            {
                if ((_curObj["ThreadCount"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The ThreadCount property specifies the number of active threads in this process. An instruction is the basic unit of execution in a processor, and a thread is the object that executes instructions. Every running process has at least one thread. This property is for computers running Windows NT only.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public uint ThreadCount {
            get {
                if ((_curObj["ThreadCount"] == null)) {
                    return Convert.ToUInt32(0);
                }
                return ((uint)(_curObj["ThreadCount"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsUserModeTimeNull {
            get
            {
                if ((_curObj["UserModeTime"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Time in user mode, in 100 nanoseconds. If this information is not available, a va" +
            "lue of 0 should be used.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public ulong UserModeTime {
            get {
                if ((_curObj["UserModeTime"] == null)) {
                    return Convert.ToUInt64(0);
                }
                return ((ulong)(_curObj["UserModeTime"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsVirtualSizeNull {
            get
            {
                if ((_curObj["VirtualSize"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The VirtualSize property specifies the current size in bytes of the virtual address space the process is using. Use of virtual address space does not necessarily imply corresponding use of either disk or main memory pages. Virtual space is finite, and by using too much, the process can limit its ability to load libraries.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public ulong VirtualSize {
            get {
                if ((_curObj["VirtualSize"] == null)) {
                    return Convert.ToUInt64(0);
                }
                return ((ulong)(_curObj["VirtualSize"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The WindowsVersion property indicates the version of Windows in which the process" +
            " is running.\nExample: 4.0")]
        public string WindowsVersion {
            get {
                return ((string)(_curObj["WindowsVersion"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsWorkingSetSizeNull {
            get
            {
                if ((_curObj["WorkingSetSize"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The amount of memory in bytes that a process needs to execute efficiently, for an operating system that uses page-based memory management. If an insufficient amount of memory is available (< working set size), thrashing will occur. If this information is not known, NULL or 0 should be entered.  If this data is provided, it could be monitored to understand a process' changing memory requirements as execution proceeds.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public ulong WorkingSetSize {
            get {
                if ((_curObj["WorkingSetSize"] == null)) {
                    return Convert.ToUInt64(0);
                }
                return ((ulong)(_curObj["WorkingSetSize"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsWriteOperationCountNull {
            get
            {
                if ((_curObj["WriteOperationCount"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The WriteOperationCount property specifies the number of write operations perform" +
            "ed.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public ulong WriteOperationCount {
            get {
                if ((_curObj["WriteOperationCount"] == null)) {
                    return Convert.ToUInt64(0);
                }
                return ((ulong)(_curObj["WriteOperationCount"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsWriteTransferCountNull {
            get
            {
                if ((_curObj["WriteTransferCount"] == null)) {
                    return true;
                }
                return false;
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The WriteTransferCount property specifies the amount of data written.")]
        [TypeConverter(typeof(WmiValueTypeConverter))]
        public ulong WriteTransferCount {
            get {
                if ((_curObj["WriteTransferCount"] == null)) {
                    return Convert.ToUInt64(0);
                }
                return ((ulong)(_curObj["WriteTransferCount"]));
            }
        }
        
        private bool CheckIfProperClass(ManagementScope mgmtScope, ManagementPath path, ObjectGetOptions optionsParam)
        {
            if (((path != null) 
                        && (string.Compare(path.ClassName, ManagementClassName, true, CultureInfo.InvariantCulture) == 0))) {
                return true;
            }
            return CheckIfProperClass(new ManagementObject(mgmtScope, path, optionsParam));
        }

        private bool CheckIfProperClass(ManagementBaseObject theObj) {
            if (((theObj != null) 
                        && (string.Compare(((string)(theObj["__CLASS"])), ManagementClassName, true, CultureInfo.InvariantCulture) == 0))) {
                return true;
            }
            Array parentClasses = ((Array)(theObj["__DERIVATION"]));
            if ((parentClasses != null)) {
                int count = 0;
                for (count = 0; (count < parentClasses.Length); count = (count + 1)) {
                    if ((string.Compare(((string)(parentClasses.GetValue(count))), ManagementClassName, true, CultureInfo.InvariantCulture) == 0)) {
                        return true;
                    }
                }
            }
            return false;
        }
        
        // Converts a given datetime in DMTF format to System.DateTime object.
        static DateTime ToDateTime(string dmtfDate) {
            DateTime initializer = DateTime.MinValue;
            int year = initializer.Year;
            int month = initializer.Month;
            int day = initializer.Day;
            int hour = initializer.Hour;
            int minute = initializer.Minute;
            int second = initializer.Second;
            long ticks = 0;
            string dmtf = dmtfDate;
            DateTime datetime = DateTime.MinValue;
            string tempString = string.Empty;
            if ((dmtf == null)) {
                throw new ArgumentOutOfRangeException();
            }
            if ((dmtf.Length == 0)) {
                throw new ArgumentOutOfRangeException();
            }
            if ((dmtf.Length != 25)) {
                throw new ArgumentOutOfRangeException();
            }
            try {
                tempString = dmtf.Substring(0, 4);
                if (("****" != tempString)) {
                    year = int.Parse(tempString);
                }
                tempString = dmtf.Substring(4, 2);
                if (("**" != tempString)) {
                    month = int.Parse(tempString);
                }
                tempString = dmtf.Substring(6, 2);
                if (("**" != tempString)) {
                    day = int.Parse(tempString);
                }
                tempString = dmtf.Substring(8, 2);
                if (("**" != tempString)) {
                    hour = int.Parse(tempString);
                }
                tempString = dmtf.Substring(10, 2);
                if (("**" != tempString)) {
                    minute = int.Parse(tempString);
                }
                tempString = dmtf.Substring(12, 2);
                if (("**" != tempString)) {
                    second = int.Parse(tempString);
                }
                tempString = dmtf.Substring(15, 6);
                if (("******" != tempString)) {
                    ticks = (long.Parse(tempString) * (TimeSpan.TicksPerMillisecond / 1000));
                }
                if (((((((((year < 0) 
                            || (month < 0)) 
                            || (day < 0)) 
                            || (hour < 0)) 
                            || (minute < 0)) 
                            || (minute < 0)) 
                            || (second < 0)) 
                            || (ticks < 0))) {
                    throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e) {
                throw new ArgumentOutOfRangeException(null, e.Message);
            }
            datetime = new DateTime(year, month, day, hour, minute, second, 0);
            datetime = datetime.AddTicks(ticks);
            TimeSpan tickOffset = TimeZone.CurrentTimeZone.GetUtcOffset(datetime);
            int utcOffset = 0;
            int offsetToBeAdjusted = 0;
            long offsetMins = tickOffset.Ticks / TimeSpan.TicksPerMinute;
            tempString = dmtf.Substring(22, 3);
            if ((tempString != "******")) {
                tempString = dmtf.Substring(21, 4);
                try {
                    utcOffset = int.Parse(tempString);
                }
                catch (Exception e) {
                    throw new ArgumentOutOfRangeException(null, e.Message);
                }
                offsetToBeAdjusted = ((int)((offsetMins - utcOffset)));
                datetime = datetime.AddMinutes(offsetToBeAdjusted);
            }
            return datetime;
        }
        
        // Converts a given System.DateTime object to DMTF datetime format.
        static string ToDmtfDateTime(DateTime date) {
            string utcString = string.Empty;
            TimeSpan tickOffset = TimeZone.CurrentTimeZone.GetUtcOffset(date);
            long offsetMins = tickOffset.Ticks / TimeSpan.TicksPerMinute;
            if ((Math.Abs(offsetMins) > 999)) {
                date = date.ToUniversalTime();
                utcString = "+000";
            }
            else {
                if ((tickOffset.Ticks >= 0)) {
                    utcString = string.Concat("+", (tickOffset.Ticks / TimeSpan.TicksPerMinute).ToString().PadLeft(3, '0'));
                }
                else {
                    string strTemp = offsetMins.ToString();
                    utcString = string.Concat("-", strTemp.Substring(1, (strTemp.Length - 1)).PadLeft(3, '0'));
                }
            }
            string dmtfDateTime = date.Year.ToString().PadLeft(4, '0');
            dmtfDateTime = string.Concat(dmtfDateTime, date.Month.ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, date.Day.ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, date.Hour.ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, date.Minute.ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, date.Second.ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, ".");
            DateTime dtTemp = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, 0);
            long microsec = ((date.Ticks - dtTemp.Ticks) 
                             * 1000) 
                            / TimeSpan.TicksPerMillisecond;
            string strMicrosec = microsec.ToString();
            if ((strMicrosec.Length > 6)) {
                strMicrosec = strMicrosec.Substring(0, 6);
            }
            dmtfDateTime = string.Concat(dmtfDateTime, strMicrosec.PadLeft(6, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, utcString);
            return dmtfDateTime;
        }
        
        private bool ShouldSerializeCreationDate() {
            if ((IsCreationDateNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeExecutionState() {
            if ((IsExecutionStateNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeHandleCount() {
            if ((IsHandleCountNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeInstallDate() {
            if ((IsInstallDateNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeKernelModeTime() {
            if ((IsKernelModeTimeNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeMaximumWorkingSetSize() {
            if ((IsMaximumWorkingSetSizeNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeMinimumWorkingSetSize() {
            if ((IsMinimumWorkingSetSizeNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeOtherOperationCount() {
            if ((IsOtherOperationCountNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeOtherTransferCount() {
            if ((IsOtherTransferCountNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializePageFaults() {
            if ((IsPageFaultsNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializePageFileUsage() {
            if ((IsPageFileUsageNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeParentProcessId() {
            if ((IsParentProcessIdNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializePeakPageFileUsage() {
            if ((IsPeakPageFileUsageNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializePeakVirtualSize() {
            if ((IsPeakVirtualSizeNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializePeakWorkingSetSize() {
            if ((IsPeakWorkingSetSizeNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializePriority() {
            if ((IsPriorityNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializePrivatePageCount() {
            if ((IsPrivatePageCountNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeProcessId() {
            if ((IsProcessIdNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeQuotaNonPagedPoolUsage() {
            if ((IsQuotaNonPagedPoolUsageNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeQuotaPagedPoolUsage() {
            if ((IsQuotaPagedPoolUsageNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeQuotaPeakNonPagedPoolUsage() {
            if ((IsQuotaPeakNonPagedPoolUsageNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeQuotaPeakPagedPoolUsage() {
            if ((IsQuotaPeakPagedPoolUsageNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeReadOperationCount() {
            if ((IsReadOperationCountNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeReadTransferCount() {
            if ((IsReadTransferCountNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeSessionId() {
            if ((IsSessionIdNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeTerminationDate() {
            if ((IsTerminationDateNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeThreadCount() {
            if ((IsThreadCountNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeUserModeTime() {
            if ((IsUserModeTimeNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeVirtualSize() {
            if ((IsVirtualSizeNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeWorkingSetSize() {
            if ((IsWorkingSetSizeNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeWriteOperationCount() {
            if ((IsWriteOperationCountNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeWriteTransferCount() {
            if ((IsWriteTransferCountNull == false)) {
                return true;
            }
            return false;
        }
        
        [Browsable(true)]
        public void CommitObject() {
            if ((_isEmbedded == false)) {
                _privateLateBoundObject.Put();
            }
        }
        
        [Browsable(true)]
        public void CommitObject(PutOptions putOptions) {
            if ((_isEmbedded == false)) {
                _privateLateBoundObject.Put(putOptions);
            }
        }
        
        private void Initialize() {
            _autoCommitProp = true;
            _isEmbedded = false;
        }
        
        private static string ConstructPath(string keyHandle) {
            string strPath = "root\\cimv2:Win32_Process";
            strPath = string.Concat(strPath, string.Concat(".Handle=", string.Concat("\"", string.Concat(keyHandle, "\""))));
            return strPath;
        }
        
        private void InitializeObject(ManagementScope mgmtScope, ManagementPath path, ObjectGetOptions getOptions) {
            Initialize();
            if ((path != null)) {
                if ((CheckIfProperClass(mgmtScope, path, getOptions) != true)) {
                    throw new ArgumentException("Class name does not match.");
                }
            }
            _privateLateBoundObject = new ManagementObject(mgmtScope, path, getOptions);
            _privateSystemProperties = new ManagementSystemProperties(_privateLateBoundObject);
            _curObj = _privateLateBoundObject;
        }
        
        // Different overloads of GetInstances() help in enumerating instances of the WMI class.
        public static ProcessCollection GetInstances() {
            return GetInstances(null, null, null);
        }
        
        public static ProcessCollection GetInstances(string condition) {
            return GetInstances(null, condition, null);
        }
        
        public static ProcessCollection GetInstances(string[] selectedProperties) {
            return GetInstances(null, null, selectedProperties);
        }
        
        public static ProcessCollection GetInstances(string condition, string[] selectedProperties) {
            return GetInstances(null, condition, selectedProperties);
        }
        
        public static ProcessCollection GetInstances(ManagementScope mgmtScope, EnumerationOptions enumOptions) {
            if ((mgmtScope == null)) {
                if ((_statMgmtScope == null)) {
                    mgmtScope = new ManagementScope();
                    mgmtScope.Path.NamespacePath = "root\\cimv2";
                }
                else {
                    mgmtScope = _statMgmtScope;
                }
            }
            ManagementPath pathObj = new ManagementPath();
            pathObj.ClassName = "Win32_Process";
            pathObj.NamespacePath = "root\\cimv2";
            ManagementClass clsObject = new ManagementClass(mgmtScope, pathObj, null);
            if ((enumOptions == null)) {
                enumOptions = new EnumerationOptions();
                enumOptions.EnsureLocatable = true;
            }
            return new ProcessCollection(clsObject.GetInstances(enumOptions));
        }
        
        public static ProcessCollection GetInstances(ManagementScope mgmtScope, string condition) {
            return GetInstances(mgmtScope, condition, null);
        }
        
        public static ProcessCollection GetInstances(ManagementScope mgmtScope, string[] selectedProperties) {
            return GetInstances(mgmtScope, null, selectedProperties);
        }
        
        public static ProcessCollection GetInstances(ManagementScope mgmtScope, string condition, string[] selectedProperties) {
            if ((mgmtScope == null)) {
                if ((_statMgmtScope == null)) {
                    mgmtScope = new ManagementScope();
                    mgmtScope.Path.NamespacePath = "root\\cimv2";
                }
                else {
                    mgmtScope = _statMgmtScope;
                }
            }
            ManagementObjectSearcher objectSearcher = new ManagementObjectSearcher(mgmtScope, new SelectQuery("Win32_Process", condition, selectedProperties));
            EnumerationOptions enumOptions = new EnumerationOptions();
            enumOptions.EnsureLocatable = true;
            objectSearcher.Options = enumOptions;
            return new ProcessCollection(objectSearcher.Get());
        }
        
        [Browsable(true)]
        public static Win32Process CreateInstance() {
            ManagementScope mgmtScope = null;
            if ((_statMgmtScope == null)) {
                mgmtScope = new ManagementScope();
                mgmtScope.Path.NamespacePath = _createdWmiNamespace;
            }
            else {
                mgmtScope = _statMgmtScope;
            }
            ManagementPath mgmtPath = new ManagementPath(_createdClassName);
            ManagementClass tmpMgmtClass = new ManagementClass(mgmtScope, mgmtPath, null);
            return new Win32Process(tmpMgmtClass.CreateInstance());
        }
        
        [Browsable(true)]
        public void Delete() {
            _privateLateBoundObject.Delete();
        }
        
        public uint AttachDebugger()
        {
            if ((_isEmbedded == false)) {
                ManagementBaseObject inParams = null;
                ManagementBaseObject outParams = _privateLateBoundObject.InvokeMethod("AttachDebugger", inParams, null);
                return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            return Convert.ToUInt32(0);
        }

        public static uint Create(string commandLine, string currentDirectory, ManagementBaseObject processStartupInformation, out uint processId) {
            bool isMethodStatic = true;
            if (isMethodStatic) {
                ManagementBaseObject inParams = null;
                ManagementPath mgmtPath = new ManagementPath(_createdClassName);
                ManagementClass classObj = new ManagementClass(_statMgmtScope, mgmtPath, null);
                bool enablePrivileges = classObj.Scope.Options.EnablePrivileges;
                classObj.Scope.Options.EnablePrivileges = true;
                inParams = classObj.GetMethodParameters("Create");
                inParams["CommandLine"] = commandLine;
                inParams["CurrentDirectory"] = currentDirectory;
                inParams["ProcessStartupInformation"] = processStartupInformation;
                ManagementBaseObject outParams = classObj.InvokeMethod("Create", inParams, null);
                processId = Convert.ToUInt32(outParams.Properties["ProcessId"].Value);
                classObj.Scope.Options.EnablePrivileges = enablePrivileges;
                return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            processId = Convert.ToUInt32(0);
            return Convert.ToUInt32(0);
        }
        
        public uint GetAvailableVirtualSize(out ulong availableVirtualSize) {
            if ((_isEmbedded == false)) {
                ManagementBaseObject inParams = null;
                bool enablePrivileges = _privateLateBoundObject.Scope.Options.EnablePrivileges;
                _privateLateBoundObject.Scope.Options.EnablePrivileges = true;
                ManagementBaseObject outParams = _privateLateBoundObject.InvokeMethod("GetAvailableVirtualSize", inParams, null);
                availableVirtualSize = Convert.ToUInt64(outParams.Properties["AvailableVirtualSize"].Value);
                _privateLateBoundObject.Scope.Options.EnablePrivileges = enablePrivileges;
                return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            availableVirtualSize = Convert.ToUInt64(0);
            return Convert.ToUInt32(0);
        }
        
        public uint GetOwner(out string domain, out string user) {
            if ((_isEmbedded == false)) {
                ManagementBaseObject inParams = null;
                bool enablePrivileges = _privateLateBoundObject.Scope.Options.EnablePrivileges;
                _privateLateBoundObject.Scope.Options.EnablePrivileges = true;
                ManagementBaseObject outParams = _privateLateBoundObject.InvokeMethod("GetOwner", inParams, null);
                domain = Convert.ToString(outParams.Properties["Domain"].Value);
                user = Convert.ToString(outParams.Properties["User"].Value);
                _privateLateBoundObject.Scope.Options.EnablePrivileges = enablePrivileges;
                return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            domain = null;
            user = null;
            return Convert.ToUInt32(0);
        }
        
        public uint GetOwnerSid(out string sid) {
            if ((_isEmbedded == false)) {
                ManagementBaseObject inParams = null;
                bool enablePrivileges = _privateLateBoundObject.Scope.Options.EnablePrivileges;
                _privateLateBoundObject.Scope.Options.EnablePrivileges = true;
                ManagementBaseObject outParams = _privateLateBoundObject.InvokeMethod("GetOwnerSid", inParams, null);
                sid = Convert.ToString(outParams.Properties["Sid"].Value);
                _privateLateBoundObject.Scope.Options.EnablePrivileges = enablePrivileges;
                return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            sid = null;
            return Convert.ToUInt32(0);
        }
        
        public uint SetPriority(int priority)
        {
            if ((_isEmbedded == false)) {
                ManagementBaseObject inParams = null;
                bool enablePrivileges = _privateLateBoundObject.Scope.Options.EnablePrivileges;
                _privateLateBoundObject.Scope.Options.EnablePrivileges = true;
                inParams = _privateLateBoundObject.GetMethodParameters("SetPriority");
                inParams["Priority"] = priority;
                ManagementBaseObject outParams = _privateLateBoundObject.InvokeMethod("SetPriority", inParams, null);
                _privateLateBoundObject.Scope.Options.EnablePrivileges = enablePrivileges;
                return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            return Convert.ToUInt32(0);
        }

        public uint Terminate(uint reason)
        {
            if ((_isEmbedded == false)) {
                ManagementBaseObject inParams = null;
                bool enablePrivileges = _privateLateBoundObject.Scope.Options.EnablePrivileges;
                _privateLateBoundObject.Scope.Options.EnablePrivileges = true;
                inParams = _privateLateBoundObject.GetMethodParameters("Terminate");
                inParams["Reason"] = reason;
                ManagementBaseObject outParams = _privateLateBoundObject.InvokeMethod("Terminate", inParams, null);
                _privateLateBoundObject.Scope.Options.EnablePrivileges = enablePrivileges;
                return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            return Convert.ToUInt32(0);
        }

        public enum ExecutionStateValues {
            
            Unknown0 = 0,
            
            Other0 = 1,
            
            Ready = 2,
            
            Running = 3,
            
            Blocked = 4,
            
            SuspendedBlocked = 5,
            
            SuspendedReady = 6,
            
            Terminated = 7,
            
            Stopped = 8,
            
            Growing = 9,
            
            NullEnumValue = 10
        }
        
        // Enumerator implementation for enumerating instances of the class.
        public class ProcessCollection : object, ICollection {
            
            private ManagementObjectCollection _privColObj;
            
            public ProcessCollection(ManagementObjectCollection objCollection) {
                _privColObj = objCollection;
            }
            
            public virtual int Count {
                get {
                    return _privColObj.Count;
                }
            }
            
            public virtual bool IsSynchronized {
                get {
                    return _privColObj.IsSynchronized;
                }
            }
            
            public virtual object SyncRoot {
                get {
                    return this;
                }
            }
            
            public virtual void CopyTo(Array array, int index) {
                _privColObj.CopyTo(array, index);
                int nCtr;
                for (nCtr = 0; (nCtr < array.Length); nCtr = (nCtr + 1)) {
                    array.SetValue(new Win32Process(((ManagementObject)(array.GetValue(nCtr)))), nCtr);
                }
            }
            
            public virtual IEnumerator GetEnumerator() {
                return new ProcessEnumerator(_privColObj.GetEnumerator());
            }
            
            public class ProcessEnumerator : object, IEnumerator {
                
                private ManagementObjectCollection.ManagementObjectEnumerator _privObjEnum;
                
                public ProcessEnumerator(ManagementObjectCollection.ManagementObjectEnumerator objEnum) {
                    _privObjEnum = objEnum;
                }
                
                public virtual object Current {
                    get {
                        return new Win32Process(((ManagementObject)(_privObjEnum.Current)));
                    }
                }
                
                public virtual bool MoveNext() {
                    return _privObjEnum.MoveNext();
                }
                
                public virtual void Reset() {
                    _privObjEnum.Reset();
                }
            }
        }
        
        // TypeConverter to handle null values for ValueType properties
        public class WmiValueTypeConverter : TypeConverter {
            
            private TypeConverter _baseConverter;
            
            private Type _baseType;
            
            public WmiValueTypeConverter(Type inBaseType) {
                _baseConverter = TypeDescriptor.GetConverter(inBaseType);
                _baseType = inBaseType;
            }
            
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType) {
                return _baseConverter.CanConvertFrom(context, srcType);
            }
            
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
                return _baseConverter.CanConvertTo(context, destinationType);
            }
            
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
                return _baseConverter.ConvertFrom(context, culture, value);
            }
            
            public override object CreateInstance(ITypeDescriptorContext context, IDictionary dictionary) {
                return _baseConverter.CreateInstance(context, dictionary);
            }
            
            public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) {
                return _baseConverter.GetCreateInstanceSupported(context);
            }
            
            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributeVar) {
                return _baseConverter.GetProperties(context, value, attributeVar);
            }
            
            public override bool GetPropertiesSupported(ITypeDescriptorContext context) {
                return _baseConverter.GetPropertiesSupported(context);
            }
            
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
                return _baseConverter.GetStandardValues(context);
            }
            
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
                return _baseConverter.GetStandardValuesExclusive(context);
            }
            
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
                return _baseConverter.GetStandardValuesSupported(context);
            }
            
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
                if ((_baseType.BaseType == typeof(Enum))) {
                    if ((value.GetType() == destinationType)) {
                        return value;
                    }
                    if ((((value == null) 
                                && (context != null)) 
                                && (context.PropertyDescriptor.ShouldSerializeValue(context.Instance) == false))) {
                        return  "NULL_ENUM_VALUE" ;
                    }
                    return _baseConverter.ConvertTo(context, culture, value, destinationType);
                }
                if (((_baseType == typeof(bool)) 
                            && (_baseType.BaseType == typeof(ValueType)))) {
                    if ((((value == null) 
                                && (context != null)) 
                                && (context.PropertyDescriptor.ShouldSerializeValue(context.Instance) == false))) {
                        return "";
                    }
                    return _baseConverter.ConvertTo(context, culture, value, destinationType);
                }
                if (((context != null) 
                            && (context.PropertyDescriptor.ShouldSerializeValue(context.Instance) == false))) {
                    return "";
                }
                return _baseConverter.ConvertTo(context, culture, value, destinationType);
            }
        }
        
        // Embedded class to represent WMI system Properties.
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class ManagementSystemProperties {
            
            private ManagementBaseObject _privateLateBoundObject;
            
            public ManagementSystemProperties(ManagementBaseObject managedObject) {
                _privateLateBoundObject = managedObject;
            }
            
            [Browsable(true)]
            public int Genus {
                get {
                    return ((int)(_privateLateBoundObject["__GENUS"]));
                }
            }
            
            [Browsable(true)]
            public string Class {
                get {
                    return ((string)(_privateLateBoundObject["__CLASS"]));
                }
            }
            
            [Browsable(true)]
            public string Superclass {
                get {
                    return ((string)(_privateLateBoundObject["__SUPERCLASS"]));
                }
            }
            
            [Browsable(true)]
            public string Dynasty {
                get {
                    return ((string)(_privateLateBoundObject["__DYNASTY"]));
                }
            }
            
            [Browsable(true)]
            public string Relpath {
                get {
                    return ((string)(_privateLateBoundObject["__RELPATH"]));
                }
            }
            
            [Browsable(true)]
            public int PropertyCount {
                get {
                    return ((int)(_privateLateBoundObject["__PROPERTY_COUNT"]));
                }
            }
            
            [Browsable(true)]
            public string[] Derivation {
                get {
                    return ((string[])(_privateLateBoundObject["__DERIVATION"]));
                }
            }
            
            [Browsable(true)]
            public string Server {
                get {
                    return ((string)(_privateLateBoundObject["__SERVER"]));
                }
            }
            
            [Browsable(true)]
            public string Namespace {
                get {
                    return ((string)(_privateLateBoundObject["__NAMESPACE"]));
                }
            }
            
            [Browsable(true)]
            public string PATH {
                get {
                    return ((string)(_privateLateBoundObject["__PATH"]));
                }
            }
        }
    }
}
