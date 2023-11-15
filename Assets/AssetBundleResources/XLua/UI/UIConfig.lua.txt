local ui_config = {}

function AddUIConfig(id,res_path,layer,full_screen)
    ui_config[id] = {
        ['name']= res_path,
        ['layer'] = layer,
        ['full_screen'] = full_screen
    }
end

function GetUIConfig(id)
    return ui_config[id]
end

AddUIConfig(
    UIPanelEnum.Panel1,
    "Panel1",
    3,
    true
)