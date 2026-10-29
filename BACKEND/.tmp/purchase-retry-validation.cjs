const fs=require('fs');const p='C:/Users/coy81/Desktop/SISTEMA/SISTEMA/Front-End.web/Front-End/src/features/purchases/components/PurchaseDetails.tsx';let s=fs.readFileSync(p,'utf8');s=s.replace('        setUncertain(true)', `        const keepOperation = ambiguous || (failure instanceof ApiError && failure.status === 409)
        setUncertain(keepOperation)
        if (!keepOperation) { sessionStorage.removeItem(storageKey); operation.current = null }`);fs.writeFileSync(p,s);
