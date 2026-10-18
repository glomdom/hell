// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace System.Runtime;

internal static class TypeCast {
    public static unsafe object CheckCastAny(MethodTable* pTargetType, object obj) {
        return obj;
    }

    public static unsafe object CheckCastInterface(MethodTable* pTargetType, object obj) {
        return obj;
    }

    public static unsafe object CheckCastClass(MethodTable* pTargetType, object obj) {
        return obj;
    }

    public static unsafe object CheckCastClassSpecial(MethodTable* pTargetType, object obj) {
        return obj;
    }

    public static unsafe object? IsInstanceOfAny(MethodTable* pTargetType, object? obj) {
        return obj;
    }

    public static unsafe object? IsInstanceOfInterface(MethodTable* pTargetType, object? obj) {
        return obj;
    }

    public static unsafe object? IsInstanceOfClass(MethodTable* pTargetType, object? obj) {
        return obj;
    }

    public static unsafe object? IsInstanceOfException(MethodTable* pTargetType, object? obj) {
        return obj;
    }
}