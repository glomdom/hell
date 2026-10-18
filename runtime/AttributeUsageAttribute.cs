// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace System;

public sealed class AttributeUsageAttribute : Attribute {
    public bool AllowMultiple { get; set; }
    public bool Inherited { get; set; } 
    
    public AttributeUsageAttribute(AttributeTargets targets) {}
}